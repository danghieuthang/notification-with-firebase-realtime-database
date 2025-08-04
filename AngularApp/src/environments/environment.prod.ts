export const environment = {
  production: true,
  apiUrl: 'https://your-production-api.com',
  firebase: {
    // Optimized: Only essential properties for Firebase Realtime Database
    // Production values will be loaded dynamically from backend (fallback only)
    projectId: 'fir-notification-b6ff2',
    databaseURL: 'https://fir-notification-b6ff2-default-rtdb.asia-southeast1.firebasedatabase.app'
    // Removed: authDomain, storageBucket, apiKey, messagingSenderId, appId, measurementId
  }
};
