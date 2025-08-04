# Firebase Realtime Database Notification Demo

A demo project showing real-time notification system using .NET 9 WebAPI + Firebase Realtime Database + Angular 20.

## What This Demo Shows

This project demonstrates:
- **Real-time notifications** that appear instantly without page refresh
- **Two-flow notification system**: User registration + Random notifications
- **Firebase Realtime Database integration** for instant data sync
- **Simple architecture**: Backend sends to Firebase → Frontend listens to Firebase

## Quick Setup

### 1. Backend Setup (.NET 9)

```bash
cd BackendAPI
dotnet restore
dotnet run
```
API runs at: `http://localhost:5100`

**Firebase Configuration:**
- Place your `firebase-service-account.json` in the `BackendAPI/` folder
- The system uses **smart configuration** with multiple options:
  1. **Auto-mode**: Extracts `project_id` from service account and builds URL automatically
  2. **Manual-mode**: Set `Firebase:DatabaseUrl` in `appsettings.json` 
  3. **Environment-mode**: Use `FIREBASE_DATABASE_URL` environment variable
- Supports multiple Firebase regions (default: `asia-southeast1`)
- No hardcoded URLs needed!

### 2. Frontend Setup (Angular 20)

```bash
cd AngularApp
npm install
npm start
```
App runs at: `http://localhost:4200`

## Demo Flow

### Step 1: User Registration
1. Open `http://localhost:4200`
2. Enter a User ID (e.g., "demo123")
3. Click **Register**
4. ✅ Welcome notification appears instantly in real-time

### Step 2: Send Random Notifications
1. Click **Send Random Notification** button
2. ✅ Random notification appears in real-time within seconds
3. Repeat to see multiple notifications

### Step 3: Real-time Experience
- All notifications appear **instantly** without page refresh
- **No polling** - Firebase pushes updates in real-time
- Notifications show with different type badges (welcome, random)

## How It Works

```
[Angular App] ←──── Real-time ────→ [Firebase Database] ←──── API calls ────→ [.NET API]
     ↑                                       ↑                                    ↑
   Listens to                            Stores                               Sends
  Firebase DB                         notifications                      notifications
```

### The Flow:
1. **User action** → Angular calls .NET API
2. **.NET API** → Saves notification to Firebase Realtime Database
3. **Firebase** → Pushes update to Angular app in real-time
4. **Angular** → Displays notification instantly

## Sequence Diagram

### User Registration Flow
```mermaid
sequenceDiagram
    participant U as User
    participant A as Angular App
    participant API as .NET API
    participant F as Firebase DB
    
    U->>A: 1. Enter User ID & Click Register
    A->>API: 2. POST /api/notification/register {"userId": "demo123"}
    API->>F: 3. Save welcome notification to /notifications/demo123/
    API-->>A: 4. Return 200 OK (no content)
    F-->>A: 5. Real-time push: New notification data
    A->>A: 6. Display welcome notification instantly
    A->>U: 7. Show "Successfully registered!" message
```

### Send Random Notification Flow
```mermaid
sequenceDiagram
    participant U as User
    participant A as Angular App
    participant API as .NET API
    participant F as Firebase DB
    
    U->>A: 1. Click "Send Random Notification"
    A->>API: 2. POST /api/notification/send-random {"userId": "demo123"}
    API->>API: 3. Generate random notification content
    API->>F: 4. Save notification to /notifications/demo123/{id}
    API-->>A: 5. Return 200 OK (no content)
    F-->>A: 6. Real-time push: New notification data
    A->>A: 7. Add notification to list (latest first)
    A->>U: 8. Show notification with type badge & animation
```

### Firebase Config Loading Flow
```mermaid
sequenceDiagram
    participant A as Angular App
    participant API as .NET API
    participant F as Firebase DB
    
    Note over A: App Initialization
    A->>API: 1. GET /api/notification/firebase-config
    API->>API: 2. Read firebase-service-account.json
    API-->>A: 3. Return minimal Firebase config (projectId, databaseURL only)
    A->>A: 4. Initialize Firebase SDK with optimized config
    A->>F: 5. Establish real-time connection
    F-->>A: 6. Real-time listener ready
    Note over A,F: Now ready to receive notifications
```

### Complete Real-time Notification Flow
```mermaid
sequenceDiagram
    participant U1 as User 1
    participant A1 as Angular App 1
    participant U2 as User 2  
    participant A2 as Angular App 2
    participant API as .NET API
    participant F as Firebase DB
    
    Note over U1,F: User 1 registers and starts listening
    U1->>A1: Register with userId "user1"
    A1->>API: POST /register {"userId": "user1"}
    API->>F: Save welcome notification
    F-->>A1: Push welcome notification
    A1->>U1: Display welcome notification
    
    Note over U2,F: User 2 registers and starts listening  
    U2->>A2: Register with userId "user2"
    A2->>API: POST /register {"userId": "user2"}
    API->>F: Save welcome notification
    F-->>A2: Push welcome notification (only to user2)
    A2->>U2: Display welcome notification
    
    Note over U1,F: User 1 sends random notification
    U1->>A1: Click "Send Random Notification"
    A1->>API: POST /send-random {"userId": "user1"}
    API->>F: Save random notification for user1
    F-->>A1: Push notification (only to user1)
    A1->>U1: Display new notification instantly
    
    Note over A2,F: User 2 doesn't receive user1's notification
    Note over A2: No update (notifications are user-specific)
```

### API Endpoints

**Register User:**
```bash
POST /api/notification/register
{ "userId": "demo123" }
```

**Send Random Notification:**
```bash
POST /api/notification/send-random  
{ "userId": "demo123" }
```

**Get Firebase Config:**
```bash
GET /api/notification/firebase-config
```

## Firebase Database Structure

```json
{
  "notifications": {
    "a1b2c3d4e5f6": {
      "notification-id-1": {
        "title": "Welcome!",
        "body": "You have successfully registered to our notification system.",
        "timestamp": "2025-08-01T10:30:00Z",
        "type": "welcome"
      },
      "notification-id-2": {
        "title": "Breaking News!",
        "body": "You have received a new message from the system.",
        "timestamp": "2025-08-01T11:15:00Z", 
        "type": "random"
      }
    }
  }
}
```

**Note:** User IDs are hashed (SHA256, first 12 characters) for privacy. For example, `"demo123"` becomes `"a1b2c3d4e5f6"`.

## Key Demo Features

✅ **Instant Real-time Updates** - No refresh needed  
✅ **Simple Two-Flow System** - Register + Send notifications  
✅ **Clean UI** - Modern Angular interface with type badges  
✅ **Firebase Integration** - Real-time database synchronization  
✅ **Centralized Config** - Firebase settings loaded from backend  
✅ **Privacy Protection** - User IDs are hashed in Firebase paths  

## Testing the Demo

### Quick Test with curl:
```bash
# Test API
curl http://localhost:5100/api/notification/test

# Register user
curl -X POST http://localhost:5100/api/notification/register \
  -H "Content-Type: application/json" \
  -d '{"userId": "demo123"}'

# Send notification
curl -X POST http://localhost:5100/api/notification/send-random \
  -H "Content-Type: application/json" \
  -d '{"userId": "demo123"}'
```

### Watch in Firebase Console:
1. Open Firebase Console → Realtime Database
2. Watch `notifications/demo123` node
3. Send notifications and see data appear in real-time

---

**This is a demo project.** For production use, add authentication, error handling, and security measures.
