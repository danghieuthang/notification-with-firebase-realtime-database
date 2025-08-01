import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { HttpClientModule } from '@angular/common/http';
import { NotificationListComponent } from './components/notification-list.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, HttpClientModule, NotificationListComponent],
  template: `
    <div class="app-container">
      <header>
        <h1>Hangfire Notification System</h1>
      </header>
      <main>
        <app-notification-list></app-notification-list>
      </main>
    </div>
  `,
  styles: [`
    .app-container {
      min-height: 100vh;
      background: #f8f9fa;
    }

    header {
      background: #343a40;
      color: white;
      padding: 20px;
      text-align: center;
    }

    header h1 {
      margin: 0;
      font-size: 2rem;
    }

    main {
      padding: 20px;
    }
  `]
})
export class App {
  title = 'Hangfire Notification System';
}
