import { Component, OnInit, signal, HostListener, ElementRef, AfterViewInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { forkJoin, Subscription } from 'rxjs';
import { CatalogService } from '../../core/services/catalog.service';
import { BasketService } from '../../core/services/basket.service';
import { CatalogItem } from '../../core/models/catalog.model';
import { CatalogItemComponent } from '../catalog/catalog-item/catalog-item';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatProgressSpinnerModule,
    CatalogItemComponent
  ],
  templateUrl: './home.html',
  styleUrl: './home.scss'
})
export class Home implements OnInit, AfterViewInit, OnDestroy {
  featuredProducts = signal<CatalogItem[]>([]);
  newArrivals = signal<CatalogItem[]>([]);
  bestSellers = signal<CatalogItem[]>([]);
  loading = signal(true);

  currentHeroSlide = signal(0);
  heroSlides = [
    {
      title: 'Summer Collection 2026',
      subtitle: 'Discover the hottest trends this season',
      image: 'https://images.unsplash.com/photo-1441986300917-64674bd600d8?w=1200&h=500&fit=crop',
      link: '/catalog?filter=new'
    },
    {
      title: 'Latest Tech Gadgets',
      subtitle: 'Find the newest smartphones and accessories',
      image: 'https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?w=1200&h=500&fit=crop',
      link: '/catalog?filter=tech'
    },
    {
      title: 'Exclusive Offers',
      subtitle: 'Get up to 50% off on selected premium items',
      image: 'https://images.unsplash.com/photo-1607082348824-0a96f2a4b9da?w=1200&h=500&fit=crop',
      link: '/catalog?filter=sale'
    }
  ];

  private heroCarouselSubscription: Subscription | null = null;
  private observedElements: IntersectionObserver | null = null;

  constructor(
    private catalogService: CatalogService,
    private basketService: BasketService,
    private elementRef: ElementRef
  ) {}

  ngOnInit(): void {
    this.loadProducts();
    this.startHeroCarousel();
  }

  ngAfterViewInit(): void {
    this.setupScrollAnimations();
  }

  ngOnDestroy(): void {
    if (this.heroCarouselSubscription) {
      this.heroCarouselSubscription.unsubscribe();
    }
    if (this.observedElements) {
      this.observedElements.disconnect();
    }
  }

  loadProducts(): void {
    forkJoin({
      topRated: this.catalogService.getTopRatedProducts(8),
      newArrivals: this.catalogService.getNewArrivals(8),
      bestSellers: this.catalogService.getBestSellers(8)
    }).subscribe({
      next: (result) => {
        this.featuredProducts.set(result.topRated.slice(0, 8));
        this.newArrivals.set(result.newArrivals.slice(0, 8));
        this.bestSellers.set(result.bestSellers.slice(0, 8));
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error loading recommendations:', error);
        this.loadFallbackProducts();
      }
    });
  }

  loadFallbackProducts(): void {
    this.catalogService.getCatalogItems(1, 24).subscribe({
      next: (result) => {
        const products = result.data;
        this.featuredProducts.set(products.slice(0, 8));
        this.newArrivals.set(products.slice(8, 16));
        this.bestSellers.set(products.slice(16, 24));
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      }
    });
  }

  onAddToBasket(item: CatalogItem): void {
    this.basketService.addItemToBasket({
      catalogItemId: item.id,
      productName: item.name,
      unitPrice: item.price,
      pictureUrl: item.pictureUri,
      quantity: 1
    }).subscribe();
  }

  startHeroCarousel(): void {
    this.heroCarouselSubscription = new Subscription(() => {
      // Empty subscription for cleanup
    });

    const carouselInterval = setInterval(() => {
      this.currentHeroSlide.set(
        (this.currentHeroSlide() + 1) % this.heroSlides.length
      );
    }, 5000);

    this.heroCarouselSubscription.add(() => clearInterval(carouselInterval));
  }

  setupScrollAnimations(): void {
    const options = {
      root: null,
      rootMargin: '0px',
      threshold: 0.1
    };

    this.observedElements = new IntersectionObserver((entries) => {
      entries.forEach(entry => {
        if (entry.isIntersecting) {
          entry.target.classList.add('animate-in');
        }
      });
    }, options);

    const sections = this.elementRef.nativeElement.querySelectorAll(
      '.products-section, .promo-banner, .categories-section, .features-section'
    );

    sections.forEach((section: Element) => {
      section.classList.add('scroll-animate');
      this.observedElements?.observe(section);
    });
  }

  @HostListener('window:scroll')
  onWindowScroll(): void {
    // Optional: Additional scroll effects can be added here
  }
}
