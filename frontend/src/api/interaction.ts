import { createApiClient } from '../utils/axios-factory';

// Create the API client specifically for interaction service
// Note: For grading methods, we need a slightly different API path
const interactionApi = createApiClient('/interactions');
const gradingApi = createApiClient('/grading-methods');

// Grading Method Interfaces
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

// Interaction Interfaces
export interface GradeInputDTO {
  componentName: string;
  value: number;
}

export interface PostInteractionRequest {
  userId: string;
  itemId: string;
  itemType: string;
  isLiked?: boolean;
  reviewText?: string;
  useComplexGrading?: boolean;
  basicGrade?: number | null;
  gradingMethodId?: string | null;
  gradeInputs?: GradeInputDTO[];
}

export interface PostInteractionResult {
  interactionCreated: boolean;
  liked: boolean;
  reviewCreated: boolean;
  graded: boolean;
  interactionId: string;
  errorMessage: string | null;
}

export interface UpdateInteractionRequest {
  interactionId: string;
  isLiked?: boolean;
  reviewText?: string;
  updateGrading?: boolean;
  useComplexGrading?: boolean;
  basicGrade?: number | null;
  gradingMethodId?: string | null;
  gradeInputs?: GradeInputDTO[];
}

export interface UpdateInteractionResult {
  interactionUpdated: boolean;
  likeUpdated: boolean;
  reviewUpdated: boolean;
  gradingUpdated: boolean;
  interactionId: string;
  errorMessage: string | null;
}

export interface RatingNormalizedDTO {
  ratingId: string;
  normalizedGrade: number;
}

export interface ReviewDTO {
  reviewId: string;
  reviewText: string;
}

export interface InteractionDetailDTO {
  aggregateId: string;
  userId: string;
  itemId: string;
  itemType: string;
  createdAt: string;
  rating?: RatingNormalizedDTO;
  review?: ReviewDTO;
  isLiked: boolean;
}

export interface GetInteractionsResult {
  interactionsEmpty: boolean;
  interactions: InteractionDetailDTO[];
}

export interface GetRatingsResult {
  ratingsEmpty: boolean;
  ratings: RatingOverviewDTO[];
}

export interface RatingOverviewDTO {
  ratingId: string;
  grade: number;
  maxGrade: number;
  minGrade: number;
  normalizedGrade: number;
}

export interface GetRatingDetailResult {
  success: boolean;
  errorMessage?: string;
  rating: RatingDetailDTO;
}

export interface RatingDetailDTO {
  ratingId: string;
  itemId: string;
  itemType: string;
  userId: string;
  createdAt: string;
  normalizedGrade: number;
  overallGrade: number;
  minPossibleGrade: number;
  maxPossibleGrade: number;
  gradingMethodId?: string;
  gradingComponent: GradedComponentDTO;
}

export interface GradedComponentDTO {
  name: string;
  currentGrade: number;
  minPossibleGrade: number;
  maxPossibleGrade: number;
}

export interface ErrorResponse {
  errorMessage: string;
  success: boolean;
}

export interface UserInteractionDetail {
  aggregateId: string;
  userId: string;
  itemId: string;
  itemType: string;
  createdAt: string;
  rating?: {
    ratingId: string;
    normalizedGrade: number;
    isComplex: boolean;
  };
  review?: {
    reviewId: string;
    reviewText: string;
  };
  isLiked: boolean;
}

const InteractionService = {
  // Grading Method Operations
  createGradingMethod: async (gradingMethod: GradingMethodCreate): Promise<GradingMethodResponse> => {
    const response = await gradingApi.post('/create', gradingMethod);
    return response.data;
  },

  getUserGradingMethods: async (userId: string): Promise<GradingMethodSummary[]> => {
    const response = await gradingApi.get(`/by-creator-id/${userId}`);
    return response.data;
  },

  getPublicGradingMethods: async (): Promise<GradingMethodSummary[]> => {
    const response = await gradingApi.get('/public-all');
    return response.data;
  },

  getGradingMethodById: async (id: string): Promise<GradingMethodDetail> => {
    const response = await gradingApi.get(`/by-id/${id}`);
    return response.data;
  },

  deleteGradingMethod: async (id: string): Promise<DeleteGradingMethodResponse> => {
    const response = await gradingApi.delete(`/by-id/${id}`);
    return response.data;
  },

  updateGradingMethod: async (id: string, gradingMethod: Omit<GradingMethodCreate, 'userId' | 'name'>): Promise<GradingMethodResponse> => {
    const response = await gradingApi.put('', {
      gradingMethodId: id,
      ...gradingMethod
    });
    return response.data;
  },

  // Interaction Operations
  createInteraction: async (interaction: PostInteractionRequest): Promise<PostInteractionResult> => {
    const response = await interactionApi.post('/create', interaction);
    return response.data;
  },

  updateInteraction: async (interaction: UpdateInteractionRequest): Promise<UpdateInteractionResult> => {
    const response = await interactionApi.put('/update', interaction);
    return response.data;
  },

  getAllInteractions: async (): Promise<GetInteractionsResult> => {
    const response = await interactionApi.get('/all');
    return response.data;
  },

  getInteractionById: async (id: string): Promise<InteractionDetailDTO> => {
    const response = await interactionApi.get(`/by-id/${id}`);
    return response.data;
  },

  deleteInteraction: async (id: string): Promise<{success: boolean, errorMessage?: string}> => {
    const response = await interactionApi.delete(`/by-id/${id}`);
    return response.data;
  },

  getAllLikes: async () => {
    const response = await interactionApi.get('/likes-all');
    return response.data;
  },

  getAllReviews: async () => {
    const response = await interactionApi.get('/reviews-all');
    return response.data;
  },

  getAllRatings: async (): Promise<GetRatingsResult> => {
    const response = await interactionApi.get('/ratings-all');
    return response.data;
  },

  getRatingById: async (id: string): Promise<GetRatingDetailResult> => {
    const response = await interactionApi.get(`/rating-by-id/${id}`);
    return response.data;
  },

  getUserInteractionsByUserId: async (userId: string): Promise<UserInteractionDetail[]> => {
    const response = await interactionApi.get(`/by-user-id/${userId}`);
    return response.data;
  },

  getItemInteractions: async (itemId: string, itemType: string): Promise<GetInteractionsResult> => {
    const response = await interactionApi.get(`/item/${itemType}/${itemId}`);
    return response.data;
  }
};

export default InteractionService;