# Frontend Environment Configuration Guide

This guide explains how the frontend environment configuration works with our terraform setup.

## Environment Files

The frontend application uses different environment-specific configuration files:

1. **`.env.development`**: Used for local development
   ```
   VITE_API_BASE_URL=http://localhost:5000
   VITE_USER_SERVICE_URL=http://localhost:5001
   VITE_CATALOG_API_URL=http://localhost:5002
   VITE_RATING_API_URL=http://localhost:5003
   ```

2. **`.env.staging`**: Used for the dev/staging environment
   ```
   VITE_API_BASE_URL=https://api-dev.academichub.net
   VITE_USER_SERVICE_URL=https://api-dev.academichub.net
   VITE_CATALOG_API_URL=https://api-dev.academichub.net
   VITE_RATING_API_URL=https://api-dev.academichub.net
   ```

3. **`.env.production`**: Used for the production environment
   ```
   VITE_API_BASE_URL=https://api.academichub.net
   VITE_USER_SERVICE_URL=https://api.academichub.net
   VITE_CATALOG_API_URL=https://api.academichub.net
   VITE_RATING_API_URL=https://api.academichub.net
   ```

## Build Scripts in package.json

The frontend project has three build scripts:

```json
"scripts": {
  "dev": "vite",
  "build": "tsc -b && vite build",
  "build:staging": "tsc -b && vite build --mode staging",
  "build:prod": "tsc -b && vite build --mode production",
  "lint": "eslint .",
  "preview": "vite preview"
}
```

## How Terraform Selects the Right Environment

When you apply your Terraform configuration, it will:

1. Check the current workspace (`dev` or `prod`)
2. Run the appropriate build command:
   - For `dev` workspace: `npm run build:staging`
   - For `prod` workspace: `npm run build:prod`
3. Upload the built files to the correct S3 bucket
4. Invalidate the CloudFront cache to ensure users get the latest version

## Usage in Your React Components

In your React components, you can access these environment variables like this:

```javascript
// Example of using environment variables in React
import axios from 'axios';

// Create an API client with the base URL from environment variables
const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  headers: {
    'Content-Type': 'application/json'
  }
});

// Example API call
function getUserProfile() {
  return apiClient.get('/api/v1/users/me');
}

function searchCatalog(query) {
  return apiClient.get(`/api/v1/catalog/search?q=${query}`);
}
```

## Checking Current Environment in React

You can check which environment your app is running in:

```javascript
// Determine which environment the app is running in
const isProduction = import.meta.env.MODE === 'production';
const isStaging = import.meta.env.MODE === 'staging';
const isDevelopment = import.meta.env.MODE === 'development';

console.log(`Running in ${import.meta.env.MODE} mode`);
```

## Testing Locally with Different Environments

To test different environments locally:

```bash
# Run with development environment (default)
npm run dev

# Preview with staging environment variables
npm run build:staging && npm run preview

# Preview with production environment variables
npm run build:prod && npm run preview
```