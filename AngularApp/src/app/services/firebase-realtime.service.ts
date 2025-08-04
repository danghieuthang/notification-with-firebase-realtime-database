import { Injectable } from '@angular/core';
import { initializeApp, FirebaseApp } from 'firebase/app';
import { getDatabase, ref, onValue, off, Database } from 'firebase/database';
import { BehaviorSubject, Observable } from 'rxjs';
import { filter } from 'rxjs/operators';
import { NotificationData } from './notification.service';

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

  constructor() { }

  private async initializeFirebase(listenUrl: string): Promise<void> {
    if (this.isInitialized) return;

    try {
      // Extract Firebase config from listenUrl
      const url = new URL(listenUrl);
      const databaseURL = `${url.protocol}//${url.host}`;
      
      // Extract projectId from hostname: fir-notification-b6ff2-default-rtdb.asia-southeast1.firebasedatabase.app
      const hostParts = url.hostname.split('.');
      const projectId = hostParts[0].replace('-default-rtdb', '');
      
      console.log('Extracted Firebase config from listenUrl:', { projectId, databaseURL });
      
      this.app = initializeApp({
        projectId: projectId,
        databaseURL: databaseURL
      });
    } catch (error) {
      console.error('Failed to extract Firebase config from listenUrl:', error);
      throw new Error('Invalid listenUrl format');
    }

    this.database = getDatabase(this.app);
    this.isInitialized = true;
    console.log('Firebase initialized successfully from listenUrl');
  }

  async subscribeToNotifications(userId: string, listenUrl?: string): Promise<void> {
    if (!listenUrl) {
      throw new Error('listenUrl is required for subscription');
    }
    
    await this.initializeFirebase(listenUrl); // Initialize Firebase with listenUrl
    this.unsubscribeFromNotifications(); // Clean up any existing subscription
    
    if (!this.database) {
      throw new Error('Firebase database is not initialized');
    }
    
    try {
      this.currentUserId = userId;
      
      // Extract path from full URL: https://...firebasedatabase.app/notifications/hash -> notifications/hash
      const url = new URL(listenUrl);
      const firebasePath = url.pathname.substring(1); // Remove leading slash
      
      console.log(`Using provided listenUrl: ${listenUrl}`);
      console.log(`Extracted Firebase path: ${firebasePath}`);
      
      // Use the Firebase path for subscription
      this.currentRef = ref(this.database, firebasePath);
      
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
    } catch (error) {
      console.error('Error subscribing to notifications:', error);
      throw error;
    }
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

  getCurrentDatabaseUrl(): string | null {
    // Not needed anymore since we extract from listenUrl
    return null;
  }
}
