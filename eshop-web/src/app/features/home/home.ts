import { Component, OnInit, signal, HostListener, ElementRef, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { CatalogService } from '../../core/services/catalog.service';
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
    CatalogItemComponent
  ],
  templateUrl: './home.html',
  styleUrl: './home.scss'
})
export class Home implements OnInit, AfterViewInit {
  featuredProducts = signal<CatalogItem[]>([]);
  newArrivals = signal<CatalogItem[]>([]);
  bestSellers = signal<CatalogItem[]>([]);
  loading = signal(true);

  currentHeroSlide = signal(0);
  heroSlides = [
    {
      title: 'Summer Collection 2026',
      subtitle: 'Discover the hottest trends',
      image: 'https://images.unsplash.com/photo-1441986300917-64674bd600d8?w=1200',
      link: '/catalog?filter=new'
    },
    {
      title: 'Tech Gadgets',
      subtitle: 'Latest smartphones and accessories',
      image: 'https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?w=1200',
      link: '/catalog?filter=tech'
    },
    {
      title: 'Special Offers',
      subtitle: 'Up to 50% off on selected items',
      image: 'https://images.unsplash.com/photo-1607082348824-0a96f2a4b9da?w=1200',
      link: '/catalog?filter=sale'
    }
  ];

  categories = [
    { name: 'Electronics', icon: 'devices', color: '#FFD700' },
    { name: 'Fashion', icon: 'checkroom', color: '#87CEEB' },
    { name: 'Home & Garden', icon: 'home', color: '#FFD700' },
    { name: 'Sports', icon: 'sports_soccer', color: '#87CEEB' },
    { name: 'Books', icon: 'menu_book', color: '#FFD700' },
    { name: 'Toys', icon: 'toys', color: '#87CEEB' },
    { name: 'Beauty', icon: 'face', color: '#FFD700' },
    { name: 'Automotive', icon: 'directions_car', color: '#87CEEB' }
  ];

  private observedElements: IntersectionObserver | null = null;

  constructor(
    private catalogService: CatalogService,
    private elementRef: ElementRef
  ) {}

  ngOnInit(): void {
    this.loadProducts();
    this.startHeroCarousel();
  }

  ngAfterViewInit(): void {
    this.setupScrollAnimations();
  }

  loadProducts(): void {
    this.catalogService.getCatalogItems(0, 8).subscribe({
      next: (result) => {
        const products = result.data;
        this.featuredProducts.set(products.slice(0, 4));
        this.newArrivals.set(products.slice(0, 6));
        this.bestSellers.set(products.slice(0, 6));
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error loading products:', error);
        this.loading.set(false);
      }
    });
  }

  startHeroCarousel(): void {
    setInterval(() => {
      this.currentHeroSlide.set(
        (this.currentHeroSlide() + 1) % this.heroSlides.length
      );
    }, 5000);
  }

  nextSlide(): void {
    this.currentHeroSlide.set(
      (this.currentHeroSlide() + 1) % this.heroSlides.length
    );
  }

  prevSlide(): void {
    this.currentHeroSlide.set(
      (this.currentHeroSlide() - 1 + this.heroSlides.length) % this.heroSlides.length
    );
  }

  scrollCarousel(direction: 'left' | 'right', container: string): void {
    const element = document.querySelector(`.${container}`);
    if (element) {
      const scrollAmount = direction === 'left' ? -300 : 300;
      element.scrollBy({ left: scrollAmount, behavior: 'smooth' });
    }
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

    // Observe all sections
    const sections = this.elementRef.nativeElement.querySelectorAll(
      '.categories-section, .products-section, .promo-banner, .category-card'
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
