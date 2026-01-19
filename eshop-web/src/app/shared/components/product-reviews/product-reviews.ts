import { Component, Input, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { ReviewService } from '../../../core/services/review.service';
import { AuthService } from '../../../core/services/auth.service';
import {
  ProductReview,
  ProductReviewStats,
  ProductReviewsWithStats
} from '../../../core/models/review.model';

@Component({
  selector: 'app-product-reviews',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDividerModule,
    MatProgressBarModule,
    MatProgressSpinnerModule,
    MatFormFieldModule,
    MatInputModule,
    MatSnackBarModule,
    MatPaginatorModule
  ],
  templateUrl: './product-reviews.html',
  styleUrl: './product-reviews.scss'
})
export class ProductReviewsComponent implements OnInit {
  @Input() productId!: string;

  reviewsData = signal<ProductReviewsWithStats | null>(null);
  loading = signal(false);
  submitting = signal(false);
  showReviewForm = signal(false);

  pageIndex = 1;
  pageSize = 5;

  reviewForm!: FormGroup;
  selectedRating = signal(0);
  hoverRating = signal(0);

  isAuthenticated = computed(() => this.authService.isAuthenticated());
  currentUser = computed(() => this.authService.currentUser());

  stats = computed(() => this.reviewsData()?.stats || null);
  reviews = computed(() => this.reviewsData()?.reviews?.data || []);
  totalReviews = computed(() => this.reviewsData()?.reviews?.count || 0);

  constructor(
    private reviewService: ReviewService,
    private authService: AuthService,
    private fb: FormBuilder,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.loadReviews();
  }

  private initForm(): void {
    this.reviewForm = this.fb.group({
      title: ['', [Validators.required, Validators.minLength(5), Validators.maxLength(200)]],
      comment: ['', [Validators.maxLength(2000)]]
    });
  }

  loadReviews(): void {
    if (!this.productId) return;

    this.loading.set(true);
    this.reviewService.getProductReviews(this.productId, this.pageIndex, this.pageSize).subscribe({
      next: (data) => {
        this.reviewsData.set(data);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error loading reviews:', error);
        this.loading.set(false);
      }
    });
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadReviews();
  }

  setRating(rating: number): void {
    this.selectedRating.set(rating);
  }

  setHoverRating(rating: number): void {
    this.hoverRating.set(rating);
  }

  clearHoverRating(): void {
    this.hoverRating.set(0);
  }

  getDisplayRating(): number {
    return this.hoverRating() || this.selectedRating();
  }

  toggleReviewForm(): void {
    if (!this.isAuthenticated()) {
      this.snackBar.open('Veuillez vous connecter pour laisser un avis', 'Fermer', {
        duration: 3000
      });
      return;
    }
    this.showReviewForm.update(show => !show);
  }

  submitReview(): void {
    if (this.reviewForm.invalid || this.selectedRating() === 0) {
      if (this.selectedRating() === 0) {
        this.snackBar.open('Veuillez sélectionner une note', 'Fermer', { duration: 3000 });
      }
      return;
    }

    this.submitting.set(true);

    const request = {
      catalogItemId: this.productId,
      rating: this.selectedRating(),
      title: this.reviewForm.get('title')?.value,
      comment: this.reviewForm.get('comment')?.value || ''
    };

    this.reviewService.createReview(request).subscribe({
      next: () => {
        this.snackBar.open('Votre avis a été publié !', 'Fermer', { duration: 3000 });
        this.submitting.set(false);
        this.showReviewForm.set(false);
        this.reviewForm.reset();
        this.selectedRating.set(0);
        this.loadReviews();
      },
      error: (error) => {
        console.error('Error submitting review:', error);
        this.snackBar.open(
          error.error?.message || 'Erreur lors de la publication de l\'avis',
          'Fermer',
          { duration: 4000 }
        );
        this.submitting.set(false);
      }
    });
  }

  voteHelpful(reviewId: string, isHelpful: boolean): void {
    this.reviewService.voteReview(reviewId, isHelpful).subscribe({
      next: () => {
        this.loadReviews();
      },
      error: (error) => {
        console.error('Error voting:', error);
      }
    });
  }

  deleteReview(reviewId: string): void {
    if (!confirm('Êtes-vous sûr de vouloir supprimer cet avis ?')) return;

    this.reviewService.deleteReview(reviewId).subscribe({
      next: () => {
        this.snackBar.open('Avis supprimé', 'Fermer', { duration: 3000 });
        this.loadReviews();
      },
      error: (error) => {
        console.error('Error deleting review:', error);
        this.snackBar.open('Erreur lors de la suppression', 'Fermer', { duration: 3000 });
      }
    });
  }

  canDeleteReview(review: ProductReview): boolean {
    const user = this.currentUser();
    return user ? user.id === review.userId : false;
  }

  formatDate(dateString: string): string {
    return this.reviewService.formatReviewDate(dateString);
  }

  getStarArray(rating: number): number[] {
    return Array(5).fill(0).map((_, i) => i + 1);
  }

  getStarClass(star: number, rating: number): string {
    if (star <= rating) return 'star-filled';
    if (star - 0.5 <= rating) return 'star-half';
    return 'star-empty';
  }
}
