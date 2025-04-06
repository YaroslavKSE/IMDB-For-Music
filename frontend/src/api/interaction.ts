import axios from 'axios';

const INTERACTION_API_URL = import.meta.env.VITE_INTERACTION_API_URL || 'http://localhost:5003';

// Create a dedicated instance for the interaction service
export const interactionApi = axios.create({
    baseURL: `${INTERACTION_API_URL}/api`,
    headers: {
        'Content-Type': 'application/json',
    },
});

// Add the auth token interceptor
interactionApi.interceptors.request.use(
    (config) => {
        const token = localStorage.getItem('token');
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => Promise.reject(error)
);

export interface GradeComponent {
    componentType: 'grade';
    name: string;
    minGrade: number;
    maxGrade: number;
    stepAmount: number;
    description?: string;
}

export interface BlockComponent {
    componentType: 'block';
    name: string;
    subComponents: (GradeComponent | BlockComponent)[];
    actions: number[]; // 0: Add, 1: Subtract, 2: Multiply, 3: Divide
    minGrade?: number;
    maxGrade?: number;
}

export interface GradingMethodCreate {
    name: string;
    userId: string;
    isPublic: boolean;
    components: (GradeComponent | BlockComponent)[];
    actions: number[]; // 0: Add, 1: Subtract, 2: Multiply, 3: Divide
}

export interface GradingMethodResponse {
    success: boolean;
    gradingMethodId?: string;
    errorMessage?: string;
}

export interface GradingMethodSummary {
    id: string;
    name: string;
    creatorId: string;
    createdAt: string;
    isPublic: boolean;
}

export interface GradingMethodDetail {
    id: string;
    name: string;
    creatorId: string;
    createdAt: string;
    isPublic: boolean;
    components: (GradeComponent | BlockComponent)[];
    actions: number[];
    minPossibleGrade?: number;
    maxPossibleGrade?: number;
}

export interface DeleteGradingMethodResponse {
    success: boolean;
    errorMessage?: string;
}

export interface ErrorResponse {
    errorMessage: string;
    success: boolean;
}

const InteractionService = {
    // Create a new grading method
    createGradingMethod: async (gradingMethod: GradingMethodCreate): Promise<GradingMethodResponse> => {
        const response = await interactionApi.post('/grading-methods/create', gradingMethod);
        return response.data;
    },

    // Get user's grading methods
    getUserGradingMethods: async (userId: string): Promise<GradingMethodSummary[]> => {
        const response = await interactionApi.get(`/grading-methods/by-creator-id/${userId}`);
        return response.data;
    },

    // Get public grading methods
    getPublicGradingMethods: async (): Promise<GradingMethodSummary[]> => {
        const response = await interactionApi.get('/grading-methods/public-all');
        return response.data;
    },

    // Get a specific grading method by ID
    getGradingMethodById: async (id: string): Promise<GradingMethodDetail> => {
        const response = await interactionApi.get(`/grading-methods/by-id/${id}`);
        return response.data;
    },

    // Delete a grading method
    deleteGradingMethod: async (id: string): Promise<DeleteGradingMethodResponse> => {
        const response = await interactionApi.delete(`/grading-methods/by-id/${id}`);
        return response.data;
    }
};

export default InteractionService;