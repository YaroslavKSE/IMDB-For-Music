import { createApiClient } from '../utils/axios-factory';

const interactionApi = createApiClient('/interactions');
const gradingApi = createApiClient('/grading-methods');
const reviewApi = createApiClient('/review-interactions');

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
  actions: number[] | string[]; // 0: Add, 1: Subtract, 2: Multiply, 3: Divide
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
  actions: number[] | string[];
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
  isComplex: boolean;
}

export interface ReviewDTO {
  reviewId: string;
  reviewText: string;
  likes: number;
  comments: number;
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
  componentType: string;
  currentGrade: number;
  minPossibleGrade: number;
  maxPossibleGrade: number;
  components?: GradedComponentDTO[];
  actions?: string[];
  description?: string;
  stepAmount?: number;
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

export interface LikeRequest {
  reviewId: string;
  userId: string;
}

export interface ReviewCommentsResult {
  comments?: ReviewComment[];
  totalCount: number;
}

export interface PostReviewComment {
  reviewId: string;
  userId: string;
  commentText: string;
}

export interface ReviewComment {
  commentId: string;
  reviewId: string;
  userId: string;
  commentedAt: string;
  commentText: string;
}

export interface ItemStats {
  itemId: string;
  totalUsersInteracted: number;
  totalLikes: number;
  totalReviews: number;
  ratingDistribution: number[];
  averageRating: number;
  hasRatings: boolean;
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

  getInteractionById: async (id: string): Promise<InteractionDetailDTO> => {
    const response = await interactionApi.get(`/by-id/${id}`);
    return response.data;
  },

  deleteInteraction: async (id: string): Promise<{success: boolean, errorMessage?: string}> => {
    const response = await interactionApi.delete(`/by-id/${id}`);
    return response.data;
  },

  getRatingById: async (id: string): Promise<RatingDetailDTO> => {
    const response = await interactionApi.get(`/rating-by-id/${id}`);
    return response.data;
  },

  getUserInteractionsByUserId: async (userId: string, limit: number = 20, offset: number = 0): Promise<{items: UserInteractionDetail[], totalCount: number}> => {
    const response = await interactionApi.get(`/by-user-id/${userId}`, {
      params: { limit, offset }
    });
    return {
      items: response.data.items,
      totalCount: response.data.totalCount
    };
  },

  getItemStats: async (itemId: string): Promise<ItemStats> => {
    const response = await interactionApi.get(`/item-stats/${itemId}`);
    if(response.status != 200){
      console.log(response.statusText);
    }
    return response.data;
  },

  getItemReviews: async (itemId: string, limit: number = 20, offset: number = 0): Promise<{items: InteractionDetailDTO[], totalCount: number}> => {
    const response = await interactionApi.get(`/reviews-by-item-id/${itemId}`, {
      params: {limit, offset}
    });
    return {
      items: response.data.items,
      totalCount: response.data.totalCount
    };
  },

  getUserItemHistory: async (userId: string, itemId: string, limit: number = 20, offset: number = 0): Promise<{items: InteractionDetailDTO[], totalCount: number}> => {
    const response = await interactionApi.get(`/by-user-and-item`, {
      params: {userId, itemId, limit, offset}
    });
    return {
      items: response.data.items,
      totalCount: response.data.totalCount
    };
  },

  getReviewComments: async (reviewId: string, limit: number, offset: number): Promise<ReviewCommentsResult> => {
    const response = await reviewApi.get('/comments', {
      params: {reviewId, limit, offset}
    })
    return response.data;
  },

  postReviewComment: async (comment: PostReviewComment): Promise<boolean> => {
    const response = await reviewApi.post('/comments', comment);
    if(response.status === 200){
      return true;
    }
    else{
      console.log(response.data);
      return false;
    }
  },

  deleteReviewComment: async (commentId: string, userId: string): Promise<boolean> => {
    const response = await reviewApi.delete(`/comments/${commentId}`, {
      params: {userId}
    });
    if(response.status === 200){
      return true;
    }
    else{
      console.log(response.data);
      return false;
    }
  },

  checkReviewLike: async (reviewId: string, userId: string): Promise<boolean> => {
    const response = await reviewApi.get('/likes/check', {
      params: {reviewId, userId}
    });
    return response.data.hasLiked;
  },

  likeReview: async (reviewId: string, userId: string): Promise<boolean> => {
    const like: LikeRequest = {
      reviewId: reviewId,
      userId: userId
    }
    const response = await reviewApi.post('/likes', like);
    if(response.status === 200) {
      return true;
    }
    else{
      console.log(response.data);
      return false;
    }
  },

  unlikeReview: async (reviewId: string, userId: string): Promise<boolean> => {
    const response = await reviewApi.delete('/likes', {
      params: {reviewId, userId}
    })
    if(response.status === 200){
      return true;
    }
    else{
      console.log(response.data);
      return false;
    }
  }
};

export default InteractionService;