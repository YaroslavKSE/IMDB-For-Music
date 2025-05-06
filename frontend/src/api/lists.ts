import { createApiClient } from '../utils/axios-factory';

// Create the API client specifically for music lists service
const listsApi = createApiClient('/music-lists');

// List interfaces
export interface ListItem {
    spotifyId: string;
    number: number;
}

export interface CreateListRequest {
    userId: string;
    listType: string;
    listName: string;
    listDescription: string;
    isRanked: boolean;
    items: ListItem[];
}

export interface UpdateListRequest {
    listId: string;
    listType: string;
    listName: string;
    listDescription: string;
    isRanked: boolean;
    items: ListItem[];
}

export interface ListResponse {
    success: boolean;
    listId?: string;
    errorMessage?: string;
}

export interface ListItemRequest {
    spotifyId: string;
    position?: number;
}

export interface ListItemResponse {
    success: boolean;
    newPosition: number;
    totalItems: number;
    errorMessage?: string;
}

export interface AddCommentRequest {
    userId: string;
    commentText: string;
}

export interface ListCommentResponse {
    success: boolean;
    comment?: {
        commentId: string;
        listId: string;
        userId: string;
        commentedAt: string;
        commentText: string;
    };
    errorMessage?: string;
}

export interface ListComment {
    commentId: string;
    listId: string;
    userId: string;
    commentedAt: string;
    commentText: string;
}

export interface ListOverview {
    listId: string;
    userId: string;
    listType: string;
    createdAt: string;
    listName: string;
    listDescription: string;
    isRanked: boolean;
    previewItems: ListItem[];
    totalItems: number;
    likes: number;
    comments: number;
    hotScore?: number;
}

export interface ListDetail {
    listId: string;
    userId: string;
    listType: string;
    createdAt: string;
    listName: string;
    listDescription: string;
    isRanked: boolean;
    items: ListItem[];
    totalItems: number;
    likes: number;
    comments: number;
}

export interface PaginatedListsResponse {
    lists: ListOverview[];
    totalCount: number;
}

export interface PaginatedCommentsResponse {
    comments: ListComment[];
    totalCount: number;
}

export interface PaginatedItemsResponse {
    items: ListItem[];
    totalCount: number;
}

export interface LikeResponse {
    success: boolean;
    like?: {
        likeId: string;
        listId: string;
        userId: string;
        likedAt: string;
    };
    errorMessage?: string;
}

const ListsService = {
    // Create a new list
    createList: async (list: CreateListRequest): Promise<ListResponse> => {
        try {
            const response = await listsApi.post('', list);
            return response.data;
        } catch (error) {
            console.error('Error creating list:', error);
            return {
                success: false,
                errorMessage: 'Failed to create list. Please try again later.'
            };
        }
    },

    // Update an existing list
    updateList: async (listId: string, list: UpdateListRequest): Promise<ListResponse> => {
        try {
            const response = await listsApi.put(`/${listId}`, list);
            return response.data;
        } catch (error) {
            console.error('Error updating list:', error);
            return {
                success: false,
                errorMessage: 'Failed to update list. Please try again later.'
            };
        }
    },

    // Delete a list
    deleteList: async (listId: string): Promise<ListResponse> => {
        try {
            const response = await listsApi.delete(`/${listId}`);
            return response.data;
        } catch (error) {
            console.error('Error deleting list:', error);
            return {
                success: false,
                errorMessage: 'Failed to delete list. Please try again later.'
            };
        }
    },

    // Get a list by ID
    getListById: async (listId: string): Promise<ListDetail | null> => {
        try {
            const response = await listsApi.get(`/${listId}`);
            return response.data;
        } catch (error) {
            console.error('Error fetching list:', error);
            return null;
        }
    },

    // Get a user's lists
    getUserLists: async (
        userId: string,
        limit: number = 10,
        offset: number = 0,
        listType?: string
    ): Promise<PaginatedListsResponse> => {
        try {
            const params: Record<string, string | number> = { limit, offset };
            if (listType) params.listType = listType;

            const response = await listsApi.get(`/by-user/${userId}`, { params });
            return response.data;
        } catch (error) {
            console.error('Error fetching user lists:', error);
            return { lists: [], totalCount: 0 };
        }
    },

    // Get lists that contain a specific Spotify ID
    getListsBySpotifyId: async (
        spotifyId: string,
        limit: number = 10,
        offset: number = 0,
        listType?: string
    ): Promise<PaginatedListsResponse> => {
        try {
            const params: Record<string, string | number> = { limit, offset };
            if (listType) params.listType = listType;

            const response = await listsApi.get(`/by-spotify-id/${spotifyId}`, { params });
            return response.data;
        } catch (error) {
            console.error('Error fetching lists by Spotify ID:', error);
            return { lists: [], totalCount: 0 };
        }
    },

    // Get list items with pagination
    getListItems: async (
        listId: string,
        limit?: number,
        offset?: number
    ): Promise<PaginatedItemsResponse> => {
        try {
            const params: Record<string, number | undefined> = {};
            if (limit !== undefined) params.limit = limit;
            if (offset !== undefined) params.offset = offset;

            const response = await listsApi.get(`/${listId}/items`, { params });
            return response.data;
        } catch (error) {
            console.error('Error fetching list items:', error);
            return { items: [], totalCount: 0 };
        }
    },

    // Insert an item into a list
    insertListItem: async (listId: string, request: ListItemRequest): Promise<ListItemResponse> => {
        try {
            const response = await listsApi.post(`/${listId}/items/insert`, request);
            return response.data;
        } catch (error) {
            console.error('Error inserting list item:', error);
            return {
                success: false,
                newPosition: -1,
                totalItems: 0,
                errorMessage: 'Item is already in the list.'
            };
        }
    },

    // Like a list
    likeList: async (listId: string, userId: string): Promise<LikeResponse> => {
        try {
            const response = await listsApi.post(`/${listId}/like`, JSON.stringify(userId), {
                headers: {
                    'Content-Type': 'application/json'
                }
            });
            return response.data;
        } catch (error) {
            console.error('Error liking list:', error);
            return {
                success: false,
                errorMessage: 'Failed to like the list.'
            };
        }
    },

    // Unlike a list
    unlikeList: async (listId: string, userId: string): Promise<ListResponse> => {
        try {
            const response = await listsApi.delete(`/${listId}/like`, {
                params: { userId }
            });
            return response.data;
        } catch (error) {
            console.error('Error unliking list:', error);
            return {
                success: false,
                errorMessage: 'Failed to unlike the list.'
            };
        }
    },

    // Check if user has liked a list
    checkUserLikedList: async (listId: string, userId: string): Promise<boolean> => {
        try {
            const response = await listsApi.get(`/${listId}/like/${userId}`);
            return response.data.hasLiked;
        } catch (error) {
            console.error('Error checking if user liked list:', error);
            return false;
        }
    },

    // Add a comment to a list
    addListComment: async (listId: string, request: AddCommentRequest): Promise<ListCommentResponse> => {
        try {
            const response = await listsApi.post(`/${listId}/comment`, request);
            return response.data;
        } catch (error) {
            console.error('Error adding list comment:', error);
            return {
                success: false,
                errorMessage: 'Failed to add comment to the list.'
            };
        }
    },

    // Delete a comment from a list
    deleteListComment: async (commentId: string, userId: string): Promise<ListResponse> => {
        try {
            const response = await listsApi.delete(`/comment/${commentId}`, {
                params: { userId }
            });
            return response.data;
        } catch (error) {
            console.error('Error deleting list comment:', error);
            return {
                success: false,
                errorMessage: 'Failed to delete the comment.'
            };
        }
    },

    // Get comments for a list with pagination
    getListComments: async (
        listId: string,
        limit?: number,
        offset?: number
    ): Promise<PaginatedCommentsResponse> => {
        try {
            const params: Record<string, number | undefined> = {};
            if (limit !== undefined) params.limit = limit;
            if (offset !== undefined) params.offset = offset;

            const response = await listsApi.get(`/${listId}/comments`, { params });
            return response.data;
        } catch (error) {
            console.error('Error fetching list comments:', error);
            return { comments: [], totalCount: 0 };
        }
    }
};

export default ListsService;