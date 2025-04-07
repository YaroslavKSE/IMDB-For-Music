import axios, { AxiosInstance } from 'axios';

// Get environment variables with fallbacks
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api/v1';

// Service paths (not full URLs)
const USER_SERVICE_PATH = '/users';   // Path segment only, not full URL
const CATALOG_SERVICE_PATH = '/catalog';
const RATING_SERVICE_PATH = '/ratings';

// For local development, use complete URLs
const LOCAL_USER_SERVICE_URL = 'http://localhost:5001/api/v1';
const LOCAL_CATALOG_API_URL = 'http://localhost:5002/api/v1';
const LOCAL_RATING_API_URL = 'http://localhost:5003/api';

// Determine if we're in local development
const isLocalDev = import.meta.env.DEV && !import.meta.env.VITE_API_BASE_URL;

// Main API instance for user service
export const api = axios.create({
  baseURL: isLocalDev ? LOCAL_USER_SERVICE_URL : `${API_BASE_URL}${USER_SERVICE_PATH}`,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Catalog API instance
export const catalogApi = axios.create({
  baseURL: isLocalDev ? LOCAL_CATALOG_API_URL : `${API_BASE_URL}${CATALOG_SERVICE_PATH}`,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Rating API instance
export const ratingApi = axios.create({
  baseURL: isLocalDev ? LOCAL_RATING_API_URL : `${API_BASE_URL}${RATING_SERVICE_PATH}`,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor for adding auth token - accepts AxiosInstance
const addAuthTokenInterceptor = (axiosInstance: AxiosInstance) => {
  axiosInstance.interceptors.request.use(
    (config) => {
      const token = localStorage.getItem('token');
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
      return config;
    },
    (error) => Promise.reject(error)
  );
};

// Response interceptor for handling errors - accepts AxiosInstance
const addResponseErrorInterceptor = (axiosInstance: AxiosInstance) => {
  axiosInstance.interceptors.response.use(
    (response) => response,
    async (error) => {
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
};

// Apply interceptors to all API instances
[api, catalogApi, ratingApi].forEach((instance) => {
  addAuthTokenInterceptor(instance);
  addResponseErrorInterceptor(instance);
});

export default api;
