export interface ProductReview {
  id: string;
  catalogItemId: string;
  userId: string;
  userDisplayName: string;
  rating: number;
  title: string;
  comment: string;
  isVerifiedPurchase: boolean;
  helpfulCount: number;
  totalVotes: number;
  createdAt: string;
}

export interface ProductReviewStats {
  catalogItemId: string;
  totalReviews: number;
  averageRating: number;
  fiveStarCount: number;
  fourStarCount: number;
  threeStarCount: number;
  twoStarCount: number;
  oneStarCount: number;
  fiveStarPercentage: number;
  fourStarPercentage: number;
  threeStarPercentage: number;
  twoStarPercentage: number;
  oneStarPercentage: number;
}

export interface PaginatedReviews {
  pageIndex: number;
  pageSize: number;
  count: number;
  data: ProductReview[];
}

export interface ProductReviewsWithStats {
  stats: ProductReviewStats;
  reviews: PaginatedReviews;
}

export interface CreateReviewRequest {
  catalogItemId: string;
  rating: number;
  title: string;
  comment: string;
}

export interface VoteReviewRequest {
  isHelpful: boolean;
}
