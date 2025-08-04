import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { ApiConfigService } from './api-config.service';
import { environment } from '../../environments/environment';

export interface FirebaseConfig {
  projectId: string;
  databaseURL: string;
  // Optimized: Only essential properties for Firebase Realtime Database
  // Removed: authDomain, storageBucket, apiKey, messagingSenderId, appId, measurementId
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
        // Store the minimal config from backend (only projectId + databaseURL)
        this.configSubject.next(config);
        console.log('Firebase config loaded (optimized):', config);
      })
    );
  }

  getCurrentConfig(): FirebaseConfig | null {
    return this.configSubject.value;
  }

  // Fallback config using environment settings (optimized)
  getFallbackConfig(): FirebaseConfig {
    return {
      projectId: environment.firebase.projectId,
      databaseURL: environment.firebase.databaseURL
      // Removed unnecessary properties for Realtime Database only usage
    };
  }
}
