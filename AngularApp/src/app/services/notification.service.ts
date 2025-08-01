import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiConfigService } from './api-config.service';

export interface NotificationData {
  id?: string; // Optional vì có thể được thêm từ Firebase key
  title: string;
  body: string;
  timestamp: string;
  type: string;
}

export interface RegisterResponse {
  success: boolean;
  message: string;
  userId: string;
}

export interface SendNotificationResponse {
  success: boolean;
  message: string;
  title: string;
  body: string;
  notificationId: string;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private httpOptions = {
    headers: new HttpHeaders({
      'Content-Type': 'application/json'
    })
  };

  constructor(
    private http: HttpClient,
    private apiConfig: ApiConfigService
  ) { }

  // Register user and get welcome notification - returns success status
  register(userId: string): Observable<any> {
    const url = `${this.apiConfig.notificationApiUrl}/register`;
    const body = { userId };
    return this.http.post<any>(url, body, this.httpOptions);
  }

  // Send random notification to user - returns void as notification will come via Firebase realtime
  sendRandomNotification(userId: string): Observable<void> {
    const url = `${this.apiConfig.notificationApiUrl}/send-random`;
    const body = { userId };
    return this.http.post<void>(url, body, this.httpOptions);
  }

  // Send notification
  sendNotification(userId: string, title: string, body: string, data?: any): Observable<string> {
    const url = `${this.apiConfig.notificationApiUrl}/send`;
    const notificationData = {
      userId,
      title,
      body,
      data
    };
    return this.http.post<string>(url, notificationData, this.httpOptions);
  }

  // Get all notifications for a user
  getNotifications(userId: string): Observable<NotificationData[]> {
    const url = `${this.apiConfig.notificationApiUrl}/user/${userId}`;
    return this.http.get<NotificationData[]>(url);
  }
}
