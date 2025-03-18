import { api } from './axios-config';

export interface RegisterParams {
  email: string;
  password: string;
  name: string;
  surname: string;
}

export interface LoginParams {
  email: string;
  password: string;
}

export interface AuthResponse {
  userId: string;
  message: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  idToken: string;
  expiresIn: number;
  tokenType: string;
}

export interface UserProfile {
  id: string;
  email: string;
  name: string;
  surname: string;
  createdAt?: string;
  updatedAt?: string;
}

const AuthService = {
  register: async (params: RegisterParams): Promise<AuthResponse> => {
    const response = await api.post('/auth/register', params);
    return response.data;
  },

  login: async (params: LoginParams): Promise<LoginResponse> => {
    const response = await api.post('/auth/login', params);
    // Store the token and refresh token in localStorage
    localStorage.setItem('token', response.data.accessToken);
    localStorage.setItem('refreshToken', response.data.refreshToken);
    return response.data;
  },

  logout: (): void => {
    // Remove tokens and user data from localStorage
    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
  },

  getCurrentUser: async (): Promise<UserProfile> => {
    const response = await api.get('/users/me');
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