import { createApiClient } from '../utils/axios-factory';

// Create the API client specifically for users
const usersApi = createApiClient('/public/users');
const subscriptionApi = createApiClient('/users/subscriptions');

export interface UserSummary {
  id: string;
  username: string;
  name: string;
  surname: string;
}

export interface PaginatedUsersResponse {
  items: UserSummary[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface UserSubscriptionResponse {
  userId: string;
  username: string;
  name: string;
  surname: string;
  subscribedAt: string;
}

export interface PaginatedSubscriptionsResponse {
  items: UserSubscriptionResponse[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface UserProfile {
  id: string;
  username: string;
  name: string;
  surname: string;
  createdAt: string;
  updatedAt: string;
}

const UsersService = {
  // Get paginated list of users
  getUsers: async (page: number = 1, pageSize: number = 20, search?: string): Promise<PaginatedUsersResponse> => {
    const params: Record<string, string | number> = { page, pageSize };
    if (search) params.search = search;
    
    const response = await usersApi.get('', { params });
    return response.data;
  },

  // Check if current user is following another user
  checkFollowingStatus: async (userId: string): Promise<boolean> => {
    const response = await subscriptionApi.get(`/check/${userId}`);
    return response.data;
  },

  // Follow a user
  followUser: async (userId: string): Promise<any> => {
    const response = await subscriptionApi.post('/subscribe', { userId });
    return response.data;
  },

  // Unfollow a user
  unfollowUser: async (userId: string): Promise<any> => {
    const response = await subscriptionApi.delete(`/unsubscribe/${userId}`);
    return response.data;
  },

  // Get user's followers
  getUserFollowers: async (page: number = 1, pageSize: number = 20): Promise<PaginatedSubscriptionsResponse> => {
    const response = await subscriptionApi.get('/followers', { 
      params: { page, pageSize }
    });
    return response.data;
  },

  // Get users that the current user is following
  getUserFollowing: async (page: number = 1, pageSize: number = 20): Promise<PaginatedSubscriptionsResponse> => {
    const response = await subscriptionApi.get('/following', { 
      params: { page, pageSize }
    });
    return response.data;
  }
};

export default UsersService;