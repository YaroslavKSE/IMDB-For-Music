import { create } from 'zustand';
import axios from 'axios';
import AuthService, { UserProfile, UpdateProfileParams } from '../api/auth';
import { handleAuth0Logout } from '../utils/auth0-config';

interface AuthState {
  user: UserProfile | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  authInitialized: boolean; // New state to track if auth has been initialized
  error: string | null;

  // Actions
  login: (email: string, password: string) => Promise<void>;
  socialLogin: (accessToken: string, provider: string) => Promise<void>;
  register: (email: string, password: string, name: string, surname: string, username: string) => Promise<void>;
  logout: () => Promise<void>;
  clearError: () => void;
  setUser: (user: UserProfile | null) => void;
  fetchUserProfile: () => Promise<void>;
  updateProfile: (params: UpdateProfileParams) => Promise<void>;
  initializeAuth: () => Promise<void>; // New action to initialize auth state
}

const useAuthStore = create<AuthState>((set, get) => ({
  user: null,
  isAuthenticated: AuthService.isAuthenticated(),
  isLoading: true, // Start with loading state true
  authInitialized: false,
  error: null,

  // Initialize authentication state
  initializeAuth: async () => {
    try {
      set({ isLoading: true });
      const hasToken = AuthService.isAuthenticated();

      if (hasToken) {
        try {
          const userProfile = await AuthService.getCurrentUser();
          set({
            user: userProfile,
            isAuthenticated: true,
            isLoading: false,
            authInitialized: true
          });
        } catch (error) {
          // If getting the user profile fails, we invalidate the auth
          console.error('Error getting user profile during init:', error);
          localStorage.removeItem('token');
          localStorage.removeItem('refreshToken');
          localStorage.removeItem('user');
          set({
            user: null,
            isAuthenticated: false,
            isLoading: false,
            authInitialized: true
          });
        }
      } else {
        set({
          isAuthenticated: false,
          isLoading: false,
          authInitialized: true
        });
      }
    } catch (error) {
      console.error('Auth initialization error:', error);
      set({
        isAuthenticated: false,
        isLoading: false,
        authInitialized: true,
        error: 'Failed to initialize authentication'
      });
    }
  },

  login: async (email: string, password: string) => {
    try {
      set({ isLoading: true, error: null });
      await AuthService.login({ email, password });
      await get().fetchUserProfile();
      set({ isAuthenticated: true, isLoading: false });
    } catch (error) {
      console.error('Login error:', error);

      let errorMessage = 'Failed to login. Please try again.';

      if (axios.isAxiosError(error)) {
        const responseData = error.response?.data;
        if (responseData && typeof responseData === 'object' && 'message' in responseData) {
          errorMessage = String(responseData.message);
        }
      } else if (error instanceof Error) {
        errorMessage = error.message;
      }

      set({
        isLoading: false,
        error: errorMessage
      });
      throw error;
    }
  },

  socialLogin: async (accessToken: string, provider: string) => {
    try {
      set({ isLoading: true, error: null });
      await AuthService.socialLogin({ accessToken, provider });
      await get().fetchUserProfile();
      set({ isAuthenticated: true, isLoading: false });
    } catch (error) {
      console.error('Social login error:', error);

      let errorMessage = `Failed to login with ${provider}. Please try again.`;

      if (axios.isAxiosError(error)) {
        const responseData = error.response?.data;
        if (responseData && typeof responseData === 'object' && 'message' in responseData) {
          errorMessage = String(responseData.message);
        }
      } else if (error instanceof Error) {
        errorMessage = error.message;
      }

      set({
        isLoading: false,
        error: errorMessage
      });
      throw error;
    }
  },

  register: async (email: string, password: string, name: string, surname: string, username: string) => {
    try {
      set({ isLoading: true, error: null });
      await AuthService.register({ email, password, name, surname, username});
      set({ isLoading: false });
    } catch (error) {
      console.error('Registration error:', error);

      let errorMessage = 'Failed to register. Please try again.';

      if (axios.isAxiosError(error)) {
        const responseData = error.response?.data;
        if (responseData && typeof responseData === 'object' && 'message' in responseData) {
          errorMessage = String(responseData.message);
        }
      } else if (error instanceof Error) {
        errorMessage = error.message;
      }

      set({
        isLoading: false,
        error: errorMessage
      });
      throw error;
    }
  },

  logout: async () => {
    try {
      set({ isLoading: true });

      // First, call our backend logout API to revoke the refresh token
      await AuthService.logout();

      // Then, log out from Auth0 directly
      await handleAuth0Logout();

      // Clear local state
      set({
        user: null,
        isAuthenticated: false,
        isLoading: false
      });
    } catch (error) {
      console.error('Logout error:', error);
      // Even if there's an error, we should reset the auth state
      set({
        user: null,
        isAuthenticated: false,
        isLoading: false
      });
    }
  },

  updateProfile: async (params: UpdateProfileParams) => {
    try {
      set({ isLoading: true, error: null });
      const updatedUser = await AuthService.updateProfile(params);
      set({
        user: updatedUser,
        isLoading: false
      });
    } catch (error) {
      console.error('Profile update error:', error);

      // Handle different error types
      let errorMessage = 'Failed to update profile. Please try again.';

      if (axios.isAxiosError(error)) {
        // Type-safe access to Axios error properties
        const responseData = error.response?.data;

        if (responseData && typeof responseData === 'object') {
          if ('message' in responseData) {
            errorMessage = String(responseData.message);
          } else if ('code' in responseData && responseData.code === 'UsernameAlreadyTaken') {
            errorMessage = 'This username is already taken. Please choose another one.';
          }
        }
      } else if (error instanceof Error) {
        // Fallback to standard Error object
        errorMessage = error.message;
      }

      set({
        isLoading: false,
        error: errorMessage
      });

      throw error;
    }
  },

  clearError: () => set({ error: null }),

  // Fixed setUser method - this replaces the need for updateUser
  setUser: (user) => set({ user }),

  fetchUserProfile: async () => {
    if (!AuthService.isAuthenticated()) {
      set({ isLoading: false, isAuthenticated: false });
      return;
    }

    try {
      set({ isLoading: true });
      const userProfile = await AuthService.getCurrentUser();
      set({ user: userProfile, isLoading: false, isAuthenticated: true });
    } catch (error) {
      console.error('Error fetching user profile:', error);

      let errorMessage = 'Failed to fetch user profile.';

      if (!axios.isAxiosError(error)) {
        if (error instanceof Error) {
          errorMessage = error.message;
        }
      } else {
        const responseData = error.response?.data;
        if (responseData && typeof responseData === 'object' && 'message' in responseData) {
          errorMessage = String(responseData.message);
        }
      }

      // If we fail to fetch the user profile, we should clear the authentication state
      localStorage.removeItem('token');
      localStorage.removeItem('refreshToken');
      localStorage.removeItem('user');

      set({
        isLoading: false,
        isAuthenticated: false,
        user: null,
        error: errorMessage
      });
    }
  }
}));

export default useAuthStore;