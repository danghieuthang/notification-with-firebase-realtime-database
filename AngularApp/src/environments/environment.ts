export const environment = {
  production: false,
  apiUrl: 'http://localhost:5100',
  firebase: {
    // Optimized: Only essential properties for Firebase Realtime Database
    // These will be loaded dynamically from backend but can serve as fallback
    projectId: 'fir-notification-b6ff2',
    databaseURL: 'https://fir-notification-b6ff2-default-rtdb.asia-southeast1.firebasedatabase.app'
    // Removed: authDomain, storageBucket, apiKey, messagingSenderId, appId, measurementId
  }
};
