import axios, { AxiosInstance, AxiosRequestConfig } from 'axios';

// Environment-specific base URLs
const getBaseUrl = (path: string): string => {
  // Get environment variables with fallbacks
  const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api/v1';

  // For local development, use complete URLs
  const isLocalDev = import.meta.env.DEV && !import.meta.env.VITE_API_BASE_URL;

  if (isLocalDev) {
    // Return the full localhost URL for the specific service
    const servicePorts: Record<string, string> = {
      '/auth': 'http://localhost:5001/api/v1/auth',
      '/users': 'http://localhost:5001/api/v1/users',
      '/public/users': 'http://localhost:5001/api/v1/public/users',
      '/users/subscriptions': 'http://localhost:5001/api/v1/users/subscriptions',
      '/users/avatars': 'http://localhost:5001/api/v1/users/avatars',
      '/catalog': 'http://localhost:5002/api/v1/catalog',
      '/interactions': 'http://localhost:5003/api/v1/interactions',
      '/review-interactions': 'http://localhost:5003/api/v1/review-interactions',
      '/grading-methods': 'http://localhost:5003/api/v1/grading-methods',
      '/users/preferences': 'http://localhost:5001/api/v1/users/preferences',
      '/music-lists': 'http://localhost:5004/api/music-lists',
    };

    return servicePorts[path] || `http://localhost:5000${path}`;
  }

  // For dev/staging and production environments
  return `${API_BASE_URL}${path}`;
};

/**
 * Creates a configured Axios instance for a specific API service
 *
 * @param path The API path for this service (e.g., '/users', '/catalog')
 * @param config Additional Axios config options
 * @returns A configured Axios instance
 */
export const createApiClient = (path: string, config?: AxiosRequestConfig): AxiosInstance => {
  const baseURL = getBaseUrl(path);

  const instance = axios.create({
    baseURL,
    headers: {
      'Content-Type': 'application/json',
    },
    ...config,
  });

  // Add auth token interceptor
  instance.interceptors.request.use(
    (config) => {
      const token = localStorage.getItem('token');
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
      return config;
    },
    (error) => Promise.reject(error)
  );

  // Add response error interceptor
  instance.interceptors.response.use(
    (response) => response,
    async (error) => {
      console.error('API Error:', error);
      // Handle 401 errors (unauthorized)
      if (error.response?.status === 401) {
        // Clear local storage and redirect to login
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        window.location.href = '/login';
      }
      return Promise.reject(error);
    }
  );

  return instance;
};

export default createApiClient;