import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NotificationService, NotificationData, RegisterResponse } from '../services/notification.service';
import { FirebaseRealtimeService } from '../services/firebase-realtime.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-notification-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="notification-container">
      <h2>Firebase Realtime Notification System</h2>
      
      <!-- Registration Section -->
      <div class="register-section" *ngIf="!isRegistered">
        <h3>Step 1: Register User</h3>
        <div class="form-group">
          <label for="userId">Enter User ID:</label>
          <input type="text" id="userId" [(ngModel)]="userId" placeholder="e.g., user123">
        </div>
        <button (click)="register()" [disabled]="!userId || loading" class="register-btn">
          {{loading ? 'Registering...' : 'Register'}}
        </button>
      </div>

      <!-- Registered User Section -->
      <div class="registered-section" *ngIf="isRegistered">
        <div class="user-info">
          <h3>✓ Registered User: {{currentUserId}}</h3>
          <p><strong>Status:</strong> {{accessUrl}}</p>
          <p><strong>Firebase Listen URL:</strong> <code class="url-code">{{firebaseListenUrl}}</code></p>
          <p><strong>Realtime Status:</strong> <span class="listening">🔴 Listening to Firebase database...</span></p>
        </div>

        <!-- Send Random Notification -->
        <div class="send-section">
          <h3>Step 2: Send Random Notification</h3>
          <p>Click the button below to send a random notification to Firebase. The notification will appear in real-time below!</p>
          <p><small><strong>Note:</strong> Backend only sends to Firebase and returns 200 OK. The notification content will appear via real-time Firebase listener.</small></p>
          <button (click)="sendRandomNotification()" [disabled]="loading" class="send-btn">
            {{loading ? 'Sending to Firebase...' : 'Send Random Notification'}}
          </button>
        </div>

        <!-- Real-time Notifications Display -->
        <div class="realtime-notifications">
          <h3>Real-time Notifications (Latest: {{realtimeNotifications.length}})</h3>
          <div class="notification-item realtime" 
               *ngFor="let notification of realtimeNotifications; let i = index"
               [class.latest]="i === 0">
            <div class="notification-header">
              <h4>🔔 {{notification.title}}</h4>
              <span class="timestamp">{{formatDate(notification.timestamp)}}</span>
              <span class="badge realtime-badge">REALTIME</span>
              <span class="type-badge" [class]="'type-' + notification.type">{{notification.type}}</span>
            </div>
            <p class="notification-body">{{notification.body}}</p>
          </div>
          
          <div class="no-notifications" *ngIf="realtimeNotifications.length === 0">
            <p>🔍 Waiting for notifications... Send a random notification to see it appear here in real-time!</p>
          </div>
        </div>

        <!-- Actions -->
        <div class="actions">
          <button (click)="disconnect()" class="disconnect-btn">Disconnect & Reset</button>
          <button (click)="loadHistoricalNotifications()" [disabled]="loading" class="load-btn">
            Load Historical Notifications
          </button>
        </div>

        <!-- Historical Notifications -->
        <div class="historical-notifications" *ngIf="historicalNotifications.length > 0">
          <h3>Historical Notifications ({{historicalNotifications.length}})</h3>
          <div class="notification-item historical" 
               *ngFor="let notification of historicalNotifications">
            <div class="notification-header">
              <h4>📋 {{notification.title}}</h4>
              <span class="timestamp">{{formatDate(notification.timestamp)}}</span>
              <span class="badge historical-badge">HISTORICAL</span>
              <span class="type-badge" [class]="'type-' + notification.type">{{notification.type}}</span>
            </div>
            <p class="notification-body">{{notification.body}}</p>
          </div>
        </div>
      </div>

      <!-- Loading indicator -->
      <div *ngIf="loading" class="loading">
        <p>⏳ {{loadingMessage}}</p>
      </div>

      <!-- Error message -->
      <div *ngIf="error" class="error">
        <p>❌ Error: {{error}}</p>
        <button (click)="clearError()">Clear</button>
      </div>

      <!-- Success message -->
      <div *ngIf="successMessage" class="success">
        <p>✅ {{successMessage}}</p>
        <button (click)="clearSuccess()">Clear</button>
      </div>
    </div>
  `,
  styles: [`
    .notification-container {
      max-width: 900px;
      margin: 0 auto;
      padding: 20px;
      font-family: Arial, sans-serif;
    }

    .register-section, .registered-section, .send-section {
      background: #f8f9fa;
      padding: 20px;
      margin-bottom: 20px;
      border-radius: 8px;
      border-left: 4px solid #007bff;
    }

    .user-info {
      background: #d4edda;
      padding: 15px;
      border-radius: 8px;
      margin-bottom: 20px;
      border-left: 4px solid #28a745;
    }

    .url-code {
      background: #f8f9fa;
      padding: 4px 8px;
      border-radius: 4px;
      font-family: 'Courier New', monospace;
      font-size: 12px;
      color: #007bff;
      border: 1px solid #dee2e6;
      word-break: break-all;
    }

    .listening {
      color: #dc3545;
      font-weight: bold;
      animation: pulse 2s infinite;
    }

    @keyframes pulse {
      0% { opacity: 1; }
      50% { opacity: 0.5; }
      100% { opacity: 1; }
    }

    .form-group {
      margin-bottom: 15px;
    }

    label {
      display: block;
      margin-bottom: 5px;
      font-weight: bold;
    }

    input {
      width: 100%;
      padding: 10px;
      border: 2px solid #ddd;
      border-radius: 4px;
      font-size: 14px;
    }

    input:focus {
      border-color: #007bff;
      outline: none;
    }

    button {
      background: #007bff;
      color: white;
      border: none;
      padding: 12px 24px;
      border-radius: 6px;
      cursor: pointer;
      font-size: 14px;
      font-weight: bold;
      transition: background 0.3s;
    }

    button:hover:not(:disabled) {
      background: #0056b3;
    }

    button:disabled {
      background: #6c757d;
      cursor: not-allowed;
    }

    .register-btn {
      background: #28a745;
    }

    .register-btn:hover:not(:disabled) {
      background: #1e7e34;
    }

    .send-btn {
      background: #fd7e14;
    }

    .send-btn:hover:not(:disabled) {
      background: #e8590c;
    }

    .disconnect-btn {
      background: #dc3545;
      margin-right: 10px;
    }

    .disconnect-btn:hover:not(:disabled) {
      background: #c82333;
    }

    .load-btn {
      background: #6c757d;
    }

    .realtime-notifications, .historical-notifications {
      margin-top: 30px;
    }

    .notification-item {
      border: 2px solid #ddd;
      border-radius: 8px;
      padding: 16px;
      margin-bottom: 12px;
      background: white;
      transition: all 0.3s;
    }

    .notification-item.realtime {
      border-left: 6px solid #28a745;
      box-shadow: 0 2px 8px rgba(40, 167, 69, 0.2);
    }

    .notification-item.realtime.latest {
      animation: newNotification 1s ease-in-out;
      border-left: 6px solid #fd7e14;
    }

    @keyframes newNotification {
      0% { transform: scale(0.95); background: #fff3cd; }
      50% { transform: scale(1.02); background: #d1ecf1; }
      100% { transform: scale(1); background: white; }
    }

    .notification-item.historical {
      border-left: 6px solid #6c757d;
    }

    .notification-item.unread {
      background: #f8f9fa;
    }

    .notification-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 10px;
      flex-wrap: wrap;
    }

    .notification-header h4 {
      margin: 0;
      color: #333;
      flex: 1;
    }

    .timestamp {
      color: #666;
      font-size: 12px;
      margin: 0 10px;
    }

    .badge {
      padding: 4px 8px;
      border-radius: 12px;
      font-size: 10px;
      font-weight: bold;
      text-transform: uppercase;
    }

    .realtime-badge {
      background: #28a745;
      color: white;
    }

    .historical-badge {
      background: #6c757d;
      color: white;
    }

    .type-badge {
      padding: 2px 6px;
      border-radius: 8px;
      font-size: 9px;
      font-weight: bold;
      text-transform: uppercase;
      margin-left: 5px;
    }

    .type-welcome {
      background: #d4edda;
      color: #155724;
    }

    .type-random {
      background: #cce7ff;
      color: #004085;
    }

    .type-notification {
      background: #f8d7da;
      color: #721c24;
    }

    .notification-body {
      color: #555;
      margin: 0 0 10px 0;
      line-height: 1.5;
    }

    .notification-data {
      background: #f8f9fa;
      padding: 10px;
      border-radius: 4px;
      margin: 10px 0;
    }

    .data-item {
      margin: 5px 0;
      font-size: 12px;
    }

    .key {
      font-weight: bold;
      color: #007bff;
    }

    .value {
      color: #333;
    }

    .notification-actions {
      text-align: right;
    }

    .mark-read-btn {
      background: #28a745;
      font-size: 12px;
      padding: 6px 12px;
    }

    .read-status {
      text-align: right;
      color: #28a745;
      font-size: 12px;
      font-weight: bold;
    }

    .no-notifications {
      text-align: center;
      color: #666;
      padding: 40px;
      background: #f8f9fa;
      border-radius: 8px;
      border: 2px dashed #ddd;
    }

    .actions {
      margin: 30px 0;
      text-align: center;
    }

    .loading {
      text-align: center;
      color: #007bff;
      font-weight: bold;
    }

    .error {
      background: #f8d7da;
      color: #721c24;
      padding: 15px;
      border-radius: 6px;
      margin: 15px 0;
      border: 1px solid #f5c6cb;
    }

    .success {
      background: #d4edda;
      color: #155724;
      padding: 15px;
      border-radius: 6px;
      margin: 15px 0;
      border: 1px solid #c3e6cb;
    }

    .error button, .success button {
      background: transparent;
      color: inherit;
      border: 1px solid currentColor;
      padding: 5px 10px;
      margin-left: 10px;
      font-size: 12px;
    }
  `]
})
export class NotificationListComponent implements OnInit, OnDestroy {
  isRegistered = false;
  userId = '';
  currentUserId = '';
  accessUrl = '';
  firebaseListenUrl = '';
  loading = false;
  loadingMessage = '';
  error = '';
  successMessage = '';
  
  realtimeNotifications: NotificationData[] = [];
  historicalNotifications: NotificationData[] = [];
  
  private realtimeSubscription?: Subscription;

  constructor(
    private notificationService: NotificationService,
    private firebaseRealtimeService: FirebaseRealtimeService
  ) { }

  ngOnInit(): void {
    // Initialize component
  }

  ngOnDestroy(): void {
    this.disconnect();
  }

  register(): void {
    if (!this.userId.trim()) return;

    this.loading = true;
    this.loadingMessage = 'Registering user and setting up realtime connection...';
    this.clearMessages();

    this.notificationService.register(this.userId).subscribe({
      next: (response: RegisterResponse) => {
        this.loading = false;
        
        // Backend now returns RegisterResponse with listenUrl
        this.isRegistered = true;
        this.currentUserId = this.userId;
        this.accessUrl = response.message || 'Registration completed - listening for notifications';
        this.firebaseListenUrl = response.listenUrl;
        
        this.successMessage = 'Successfully registered! Welcome notification sent to Firebase.';
        
        // Start listening to realtime notifications
        this.startRealtimeListening();
      },
      error: (error) => {
        this.loading = false;
        this.error = 'Failed to register: ' + (error.message || 'Unknown error');
        console.error('Registration error:', error);
      }
    });
  }

  private async startRealtimeListening(): Promise<void> {
    try {
      // Subscribe to realtime notifications using listenUrl from register response
      // listenUrl contains everything needed: databaseURL + Firebase path with hash
      await this.firebaseRealtimeService.subscribeToNotifications(this.currentUserId, this.firebaseListenUrl);
      
      // Listen for new notifications
      this.realtimeSubscription = this.firebaseRealtimeService.getNotificationUpdates().subscribe({
        next: (notification: NotificationData) => {
          // Add to the beginning of the array (latest first)
          const existingIndex = this.realtimeNotifications.findIndex(n => n.id === notification.id);
          if (existingIndex === -1) {
            this.realtimeNotifications.unshift(notification);
            this.successMessage = `🔔 New notification received: "${notification.title}"`;
            
            // Auto-clear success message after 5 seconds
            setTimeout(() => {
              if (this.successMessage.includes(notification.title)) {
                this.successMessage = '';
              }
            }, 5000);
          }
        },
        error: (error) => {
          this.error = 'Realtime connection error: ' + error.message;
          console.error('Realtime error:', error);
        }
      });
    } catch (error: any) {
      this.error = 'Failed to initialize Firebase: ' + error.message;
      console.error('Firebase initialization error:', error);
    }
  }

  sendRandomNotification(): void {
    if (!this.currentUserId) return;

    this.loading = true;
    this.loadingMessage = 'Sending random notification...';
    this.clearMessages();

    this.notificationService.sendRandomNotification(this.currentUserId).subscribe({
      next: () => {
        this.loading = false;
        this.successMessage = 'Notification sent to Firebase! Watch for it to appear in real-time above...';
      },
      error: (error) => {
        this.loading = false;
        this.error = 'Failed to send notification: ' + (error.message || 'Unknown error');
        console.error('Send notification error:', error);
      }
    });
  }

  loadHistoricalNotifications(): void {
    if (!this.currentUserId) return;

    this.loading = true;
    this.loadingMessage = 'Loading historical notifications...';

    this.notificationService.getNotifications(this.currentUserId).subscribe({
      next: (notifications) => {
        this.loading = false;
        this.historicalNotifications = notifications || [];
        this.successMessage = `Loaded ${this.historicalNotifications.length} historical notifications`;
      },
      error: (error) => {
        this.loading = false;
        this.error = 'Failed to load historical notifications: ' + (error.message || 'Unknown error');
        console.error('Load notifications error:', error);
      }
    });
  }

  disconnect(): void {
    this.isRegistered = false;
    this.currentUserId = '';
    this.userId = '';
    this.accessUrl = '';
    this.firebaseListenUrl = '';
    this.realtimeNotifications = [];
    this.historicalNotifications = [];
    
    // Unsubscribe from realtime updates
    if (this.realtimeSubscription) {
      this.realtimeSubscription.unsubscribe();
    }
    
    this.firebaseRealtimeService.unsubscribeFromNotifications();
    
    this.clearMessages();
    this.successMessage = 'Disconnected successfully';
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString() + ' ' + date.toLocaleTimeString();
  }

  clearError(): void {
    this.error = '';
  }

  clearSuccess(): void {
    this.successMessage = '';
  }

  private clearMessages(): void {
    this.error = '';
    this.successMessage = '';
  }
}
