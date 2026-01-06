import { Component, OnInit, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

interface HeroSlide {
  id: string;
  title: string;
  subtitle?: string;
  badge?: string;
  imageUrl: string;
  ctaText: string;
  ctaLink: string;
  secondaryCtaText?: string;
  secondaryCtaLink?: string;
}

interface QuickLink {
  id: string;
  title: string;
  subtitle: string;
  icon: string;
  route: string;
}

@Component({
  selector: 'app-hero-section',
  imports: [
    CommonModule,
    RouterLink,
    MatButtonModule,
    MatIconModule
  ],
  templateUrl: './hero-section.html',
  styleUrl: './hero-section.scss',
})
export class HeroSection implements OnInit, OnDestroy {
  currentSlide = signal(0);
  private autoPlayInterval: any;
  private readonly AUTO_PLAY_DELAY = 5000; // 5 seconds

  slides: HeroSlide[] = [
    {
      id: '1',
      badge: 'NEW ARRIVAL',
      title: 'Summer Collection 2026',
      subtitle: 'Discover the latest trends in fashion',
      imageUrl: 'https://images.unsplash.com/photo-1441984904996-e0b6ba687e04?w=1600&h=600&fit=crop',
      ctaText: 'Shop Now',
      ctaLink: '/catalog',
      secondaryCtaText: 'Learn More',
      secondaryCtaLink: '/catalog'
    },
    {
      id: '2',
      badge: 'LIMITED OFFER',
      title: 'Up to 50% Off',
      subtitle: 'Don\'t miss out on our biggest sale of the year',
      imageUrl: 'https://images.unsplash.com/photo-1607082348824-0a96f2a4b9da?w=1600&h=600&fit=crop',
      ctaText: 'View Deals',
      ctaLink: '/catalog',
      secondaryCtaText: 'All Categories',
      secondaryCtaLink: '/catalog'
    },
    {
      id: '3',
      badge: 'BESTSELLERS',
      title: 'Top Rated Products',
      subtitle: 'Customer favorites you\'ll love',
      imageUrl: 'https://images.unsplash.com/photo-1607083206968-13611e3d76db?w=1600&h=600&fit=crop',
      ctaText: 'Explore Now',
      ctaLink: '/catalog'
    }
  ];

  quickLinks: QuickLink[] = [
    {
      id: '1',
      title: 'Electronics',
      subtitle: 'Latest gadgets',
      icon: 'devices',
      route: '/catalog'
    },
    {
      id: '2',
      title: 'Fashion',
      subtitle: 'Trending styles',
      icon: 'checkroom',
      route: '/catalog'
    },
    {
      id: '3',
      title: 'Home & Kitchen',
      subtitle: 'Best deals',
      icon: 'home',
      route: '/catalog'
    },
    {
      id: '4',
      title: 'Sports',
      subtitle: 'Gear up',
      icon: 'sports_soccer',
      route: '/catalog'
    }
  ];

  ngOnInit(): void {
    this.startAutoPlay();
  }

  ngOnDestroy(): void {
    this.stopAutoPlay();
  }

  nextSlide(): void {
    this.currentSlide.update(current =>
      current === this.slides.length - 1 ? 0 : current + 1
    );
    this.resetAutoPlay();
  }

  previousSlide(): void {
    this.currentSlide.update(current =>
      current === 0 ? this.slides.length - 1 : current - 1
    );
    this.resetAutoPlay();
  }

  goToSlide(index: number): void {
    this.currentSlide.set(index);
    this.resetAutoPlay();
  }

  private startAutoPlay(): void {
    this.autoPlayInterval = setInterval(() => {
      this.nextSlide();
    }, this.AUTO_PLAY_DELAY);
  }

  private stopAutoPlay(): void {
    if (this.autoPlayInterval) {
      clearInterval(this.autoPlayInterval);
    }
  }

  private resetAutoPlay(): void {
    this.stopAutoPlay();
    this.startAutoPlay();
  }
}
