import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { ApiConfigService } from './api-config.service';
import { environment } from '../../environments/environment';

export interface FirebaseConfig {
  projectId: string;
  databaseURL: string;
  authDomain: string;
  storageBucket: string;
  apiKey?: string;
  messagingSenderId?: string;
  appId?: string;
  measurementId?: string;
}

@Injectable({
  providedIn: 'root'
})
export class FirebaseConfigService {
  private configSubject = new BehaviorSubject<FirebaseConfig | null>(null);
  public config$ = this.configSubject.asObservable();

  constructor(
    private http: HttpClient,
    private apiConfig: ApiConfigService
  ) { }

  loadFirebaseConfig(): Observable<FirebaseConfig> {
    return this.http.get<FirebaseConfig>(this.apiConfig.firebaseConfigUrl).pipe(
      tap(config => {
        // Merge backend config with client-specific config
        const fullConfig: FirebaseConfig = {
          ...config,
          // These are client-specific and safe to include
          apiKey: 'AIzaSyBCqqqTnYdJJpSu3dGpSajDCJK9FdXJ9vM',
          messagingSenderId: '950969463050',
          appId: '1:950969463050:web:9e08a75ba6d4c0f3adccbb',
          measurementId: 'G-PS4MK8MPFR'
        };
        
        this.configSubject.next(fullConfig);
        console.log('Firebase config loaded and merged:', fullConfig);
      })
    );
  }

  getCurrentConfig(): FirebaseConfig | null {
    return this.configSubject.value;
  }

  // Fallback config using environment settings
  getFallbackConfig(): FirebaseConfig {
    return {
      projectId: environment.firebase.projectId,
      databaseURL: environment.firebase.databaseURL,
      authDomain: environment.firebase.authDomain,
      storageBucket: environment.firebase.storageBucket,
      apiKey: environment.firebase.apiKey,
      messagingSenderId: environment.firebase.messagingSenderId,
      appId: environment.firebase.appId,
      measurementId: environment.firebase.measurementId
    };
  }
}
