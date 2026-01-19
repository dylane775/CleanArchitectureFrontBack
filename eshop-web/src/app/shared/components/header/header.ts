import { Component, computed, signal, OnInit, OnDestroy, ElementRef, ViewChild, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatBadgeModule } from '@angular/material/badge';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Subject, debounceTime, distinctUntilChanged, switchMap, of, takeUntil, catchError } from 'rxjs';
import { AuthService } from '../../../core/services/auth.service';
import { BasketService } from '../../../core/services/basket.service';
import { CatalogService } from '../../../core/services/catalog.service';
import { WishlistService } from '../../../core/services/wishlist.service';
import { SearchSuggestion } from '../../../core/models/catalog.model';
import { environment } from '../../../../environments/environment';
import { NotificationBellComponent } from '../notification-bell/notification-bell';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatBadgeModule,
    MatMenuModule,
    MatDividerModule,
    MatInputModule,
    MatFormFieldModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    NotificationBellComponent
  ],
  templateUrl: './header.html',
  styleUrl: './header.scss'
})
export class HeaderComponent implements OnInit, OnDestroy {
  @ViewChild('searchInput') searchInput!: ElementRef;

  isAuthenticated = computed(() => this.authService.isAuthenticated());
  currentUser = computed(() => this.authService.currentUser());
  basketItemCount = computed(() => this.basketService.itemCount());
  wishlistCount = computed(() => this.wishlistService.wishlistCount());
  searchQuery = signal('');

  // Auto-complétion
  suggestions = signal<SearchSuggestion[]>([]);
  showSuggestions = signal(false);
  isLoadingSuggestions = signal(false);
  selectedSuggestionIndex = signal(-1);

  private searchSubject = new Subject<string>();
  private destroy$ = new Subject<void>();

  constructor(
    private authService: AuthService,
    private basketService: BasketService,
    private catalogService: CatalogService,
    private wishlistService: WishlistService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    // Configuration du debounce pour l'auto-complétion
    this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      switchMap(query => {
        if (query.length < 2) {
          return of([]);
        }
        this.isLoadingSuggestions.set(true);
        return this.catalogService.getSearchSuggestions(query).pipe(
          catchError(() => of([]))
        );
      }),
      takeUntil(this.destroy$)
    ).subscribe(suggestions => {
      this.suggestions.set(suggestions);
      this.isLoadingSuggestions.set(false);
      this.showSuggestions.set(suggestions.length > 0);
      this.selectedSuggestionIndex.set(-1);
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // Fermer les suggestions en cliquant à l'extérieur
  @HostListener('document:click', ['$event'])
  onDocumentClick(event: Event): void {
    const target = event.target as HTMLElement;
    if (!target.closest('.search-section')) {
      this.showSuggestions.set(false);
    }
  }

  onSearchInput(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.searchQuery.set(value);
    this.searchSubject.next(value);

    if (value.length < 2) {
      this.suggestions.set([]);
      this.showSuggestions.set(false);
    }
  }

  onSearchFocus(): void {
    if (this.suggestions().length > 0) {
      this.showSuggestions.set(true);
    }
  }

  onKeyDown(event: KeyboardEvent): void {
    const suggestions = this.suggestions();

    switch (event.key) {
      case 'ArrowDown':
        event.preventDefault();
        if (this.selectedSuggestionIndex() < suggestions.length - 1) {
          this.selectedSuggestionIndex.update(i => i + 1);
        }
        break;
      case 'ArrowUp':
        event.preventDefault();
        if (this.selectedSuggestionIndex() > -1) {
          this.selectedSuggestionIndex.update(i => i - 1);
        }
        break;
      case 'Enter':
        event.preventDefault();
        if (this.selectedSuggestionIndex() >= 0 && suggestions[this.selectedSuggestionIndex()]) {
          this.selectSuggestion(suggestions[this.selectedSuggestionIndex()]);
        } else {
          this.onSearch();
        }
        break;
      case 'Escape':
        this.showSuggestions.set(false);
        break;
    }
  }

  selectSuggestion(suggestion: SearchSuggestion): void {
    this.showSuggestions.set(false);
    this.searchQuery.set(suggestion.name);
    this.router.navigate(['/product', suggestion.id]);
  }

  getSuggestionImageUrl(suggestion: SearchSuggestion): string {
    if (!suggestion.pictureUri) {
      return 'https://via.placeholder.com/40x40?text=No+Image';
    }
    if (suggestion.pictureUri.startsWith('http')) {
      return suggestion.pictureUri;
    }
    const catalogBaseUrl = environment.catalogApiUrl.replace('/api', '');
    return `${catalogBaseUrl}${suggestion.pictureUri}`;
  }

  clearSearch(): void {
    this.searchQuery.set('');
    this.suggestions.set([]);
    this.showSuggestions.set(false);
  }

  getUserDisplayName(): string {
    const user = this.currentUser();
    if (!user) return '';

    if (user.firstName && user.lastName) {
      return `${user.firstName} ${user.lastName}`;
    }
    if (user.firstName) {
      return user.firstName;
    }
    return user.email;
  }

  isAdmin(): boolean {
    const user = this.currentUser();
    return user?.roles?.includes('Admin') ?? false;
  }

  navigateToProfile(): void {
    this.router.navigate(['/profile']);
  }

  navigateToOrders(): void {
    this.router.navigate(['/orders']);
  }

  logout(): void {
    this.authService.logout();
    this.snackBar.open('You have been logged out successfully', '✓', {
      duration: 2500,
      horizontalPosition: 'end',
      verticalPosition: 'top',
      panelClass: ['success-snackbar']
    });
    this.router.navigate(['/auth/login']);
  }

  onSearch(): void {
    this.showSuggestions.set(false);
    if (this.searchQuery().trim()) {
      this.router.navigate(['/catalog'], {
        queryParams: { search: this.searchQuery() }
      });
    }
  }
}
