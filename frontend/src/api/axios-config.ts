import axios, { AxiosInstance } from 'axios';

// Base API URLs for different services
const USER_SERVICE_URL = import.meta.env.VITE_USER_SERVICE_URL || 'http://localhost:5001';
const CATALOG_API_URL = import.meta.env.VITE_CATALOG_API_URL || 'http://localhost:5010';
const RATING_API_URL = import.meta.env.VITE_RATING_API_URL || 'http://localhost:5003';

// Main API instance for user service
export const api = axios.create({
  baseURL: `${USER_SERVICE_URL}/api/v1`,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Catalog API instance
export const catalogApi = axios.create({
  baseURL: `${CATALOG_API_URL}/api/v1`,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Rating API instance
export const ratingApi = axios.create({
  baseURL: `${RATING_API_URL}/api/v1`,
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