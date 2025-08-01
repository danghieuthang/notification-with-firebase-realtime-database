import { Injectable } from '@angular/core';
import { initializeApp, FirebaseApp } from 'firebase/app';
import { getDatabase, ref, onValue, off, Database } from 'firebase/database';
import { BehaviorSubject, Observable } from 'rxjs';
import { filter, switchMap } from 'rxjs/operators';
import { NotificationData } from './notification.service';
import { FirebaseConfigService, FirebaseConfig } from './firebase-config.service';

@Injectable({
  providedIn: 'root'
})
export class FirebaseRealtimeService {
  private app: FirebaseApp | null = null;
  private database: Database | null = null;
  
  private notificationUpdates = new BehaviorSubject<NotificationData | null>(null);
  private currentUserId: string | null = null;
  private currentRef: any = null;
  private lastNotificationId: string | null = null;
  private isInitialized = false;

  constructor(private firebaseConfigService: FirebaseConfigService) { }

  private async initializeFirebase(): Promise<void> {
    if (this.isInitialized) return;

    try {
      // Load config from backend API first
      await this.firebaseConfigService.loadFirebaseConfig().toPromise();
      const config = this.firebaseConfigService.getCurrentConfig();
      
      if (config) {
        console.log('Using Firebase config from backend API:', config);
        this.app = initializeApp({
          projectId: config.projectId,
          databaseURL: config.databaseURL,
          authDomain: config.authDomain,
          storageBucket: config.storageBucket,
          apiKey: config.apiKey,
          messagingSenderId: config.messagingSenderId,
          appId: config.appId,
          measurementId: config.measurementId
        });
      } else {
        throw new Error('Failed to load config from backend');
      }
    } catch (error) {
      console.warn('Failed to load config from backend, using fallback:', error);
      // Fallback with complete config
      const fallbackConfig = this.firebaseConfigService.getFallbackConfig();
      console.log('Using fallback Firebase config:', fallbackConfig);
      
      this.app = initializeApp(fallbackConfig);
    }

    this.database = getDatabase(this.app);
    this.isInitialized = true;
    console.log('Firebase initialized successfully');
  }

  async subscribeToNotifications(userId: string): Promise<void> {
    await this.initializeFirebase(); // Ensure Firebase is initialized
    this.unsubscribeFromNotifications(); // Clean up any existing subscription
    
    if (!this.database) {
      throw new Error('Firebase database is not initialized');
    }
    
    this.currentUserId = userId;
    this.currentRef = ref(this.database, `notifications/${userId}`);
    
    onValue(this.currentRef, (snapshot) => {
      const data = snapshot.val();
      if (data) {
        // Convert Firebase object to array and get the latest notification
        const notifications = Object.keys(data).map(key => ({
          id: key,
          ...data[key]
        }));
        
        // Sort by timestamp and get the latest
        notifications.sort((a, b) => new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime());
        
        if (notifications.length > 0) {
          const latestNotification = notifications[0] as NotificationData;
          
          // Only emit if this is a new notification (different from the last one we processed)
          if (this.lastNotificationId !== (latestNotification.id || null)) {
            this.lastNotificationId = latestNotification.id || null;
            this.notificationUpdates.next(latestNotification);
          }
        }
      }
    });
  }

  unsubscribeFromNotifications(userId?: string): void {
    if (this.currentRef) {
      off(this.currentRef);
      this.currentRef = null;
    }
    this.currentUserId = null;
    this.lastNotificationId = null;
  }

  getNotificationUpdates(): Observable<NotificationData> {
    return this.notificationUpdates.asObservable().pipe(
      // Filter out null values
      filter(notification => notification !== null)
    ) as Observable<NotificationData>;
  }
}
