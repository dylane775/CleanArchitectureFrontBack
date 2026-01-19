import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  ProductReview,
  ProductReviewsWithStats,
  CreateReviewRequest,
  VoteReviewRequest
} from '../models/review.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ReviewService {
  private readonly apiUrl = environment.catalogApiUrl;

  constructor(private http: HttpClient) {}

  /**
   * Récupère les avis d'un produit avec les statistiques
   */
  getProductReviews(
    productId: string,
    pageIndex: number = 1,
    pageSize: number = 10
  ): Observable<ProductReviewsWithStats> {
    const params = new HttpParams()
      .set('pageIndex', pageIndex.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<ProductReviewsWithStats>(
      `${this.apiUrl}/reviews/product/${productId}`,
      { params }
    );
  }

  /**
   * Récupère les avis d'un utilisateur
   */
  getUserReviews(userId: string): Observable<ProductReview[]> {
    return this.http.get<ProductReview[]>(`${this.apiUrl}/reviews/user/${userId}`);
  }

  /**
   * Crée un nouvel avis
   */
  createReview(request: CreateReviewRequest): Observable<ProductReview> {
    return this.http.post<ProductReview>(`${this.apiUrl}/reviews`, request);
  }

  /**
   * Supprime un avis
   */
  deleteReview(reviewId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/reviews/${reviewId}`);
  }

  /**
   * Vote sur l'utilité d'un avis
   */
  voteReview(reviewId: string, isHelpful: boolean): Observable<ProductReview> {
    const request: VoteReviewRequest = { isHelpful };
    return this.http.post<ProductReview>(`${this.apiUrl}/reviews/${reviewId}/vote`, request);
  }

  /**
   * Calcule le nombre d'étoiles pleines, demi et vides pour l'affichage
   */
  getStarDisplay(rating: number): { full: number; half: boolean; empty: number } {
    const full = Math.floor(rating);
    const half = rating % 1 >= 0.5;
    const empty = 5 - full - (half ? 1 : 0);
    return { full, half, empty };
  }

  /**
   * Formate la date d'un avis
   */
  formatReviewDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString('fr-FR', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  }
}
