import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ApiConfigService {
  // Centralized API configuration using environment
  private readonly baseUrl = environment.apiUrl;
  
  get notificationApiUrl(): string {
    return `${this.baseUrl}/api/notification`;
  }
  
  get firebaseConfigUrl(): string {
    return `${this.baseUrl}/api/notification/firebase-config`;
  }
}
