import { createApiClient } from '../utils/axios-factory';

// Create the API client specifically for auth/user service
const authApi = createApiClient('/auth');
const userApi = createApiClient('/users');

export interface RegisterParams {
  email: string;
  password: string;
  name: string;
  surname: string;
  username: string;
}

export interface LoginParams {
  email: string;
  password: string;
}

export interface SocialLoginParams {
  accessToken: string;
  provider: string;
}

export interface AuthResponse {
  userId: string;
  message: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  tokenType: string;
}

export interface UserProfile {
  id: string;
  email: string;
  name: string;
  surname: string;
  username: string;
  createdAt?: string;
  updatedAt?: string;
}

const AuthService = {
  register: async (params: RegisterParams): Promise<AuthResponse> => {
    const response = await authApi.post('/register', params);
    return response.data;
  },

  login: async (params: LoginParams): Promise<LoginResponse> => {
    const response = await authApi.post('/login', params);
    // Store the token and refresh token in localStorage
    localStorage.setItem('token', response.data.accessToken);
    localStorage.setItem('refreshToken', response.data.refreshToken);
    return response.data;
  },

  // Handle social login with Auth0 (including Google)
  socialLogin: async (params: SocialLoginParams): Promise<LoginResponse> => {
    const response = await authApi.post('/social-login', params);
    // Store the token and refresh token in localStorage
    localStorage.setItem('token', response.data.accessToken);
    localStorage.setItem('refreshToken', response.data.refreshToken);
    return response.data;
  },

  logout: async (): Promise<void> => {
    const refreshToken = localStorage.getItem('refreshToken');

    if (refreshToken) {
      try {
        // Call the logout endpoint to revoke the token on server side
        await authApi.post('/auth/logout', { refreshToken });
      } catch (error) {
        console.error('Error during logout:', error);
      }
    }

    // Always clean up local storage regardless of server response
    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
  },

  getCurrentUser: async (): Promise<UserProfile> => {
    const response = await userApi.get('/me');
    // Cache user profile
    localStorage.setItem('user', JSON.stringify(response.data));
    return response.data;
  },

  isAuthenticated: (): boolean => {
    return !!localStorage.getItem('token');
  },

  getToken: (): string | null => {
    return localStorage.getItem('token');
  },
};

export default AuthService;