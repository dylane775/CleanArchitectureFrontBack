import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

/**
 * Footer Component - E-commerce footer with navigation, social links, and payment methods
 *
 * Features:
 * - Semantic HTML5 structure (footer, nav, role attributes)
 * - WCAG 2.1 AA compliant (accessibility)
 * - SEO optimized (internal links, proper headings)
 * - Responsive design (desktop, tablet, mobile)
 * - Amazon-inspired design
 */
@Component({
  selector: 'app-footer',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatIconModule,
    MatButtonModule
  ],
  templateUrl: './footer.html',
  styleUrl: './footer.scss'
})
export class FooterComponent {
  currentYear = new Date().getFullYear();

  /**
   * Social media links configuration
   * Icons use Material Icons font
   */
  socialLinks = [
    { icon: 'facebook', url: 'https://facebook.com', name: 'Facebook' },
    { icon: 'X', url: 'https://twitter.com', name: 'Twitter' },
    { icon: 'instagram', url: 'https://instagram.com', name: 'Instagram' },
    { icon: 'linkedin', url: 'https://linkedin.com', name: 'LinkedIn' }
  ];

  /**
   * Scroll to top of the page with smooth animation
   * Triggered by the "Back to top" button
   */
  scrollToTop(): void {
    window.scrollTo({
      top: 0,
      behavior: 'smooth'
    });
  }
}
