import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

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
    FormsModule,
    MatIconModule,
    MatButtonModule,
    MatSnackBarModule
  ],
  templateUrl: './footer.html',
  styleUrl: './footer.scss'
})
export class FooterComponent {
  currentYear = new Date().getFullYear();

  // Newsletter signals
  newsletterEmail = '';
  isSubscribing = signal(false);
  newsletterMessage = signal('');
  newsletterSuccess = signal(false);

  /**
   * Social media links configuration
   * Icons use Material Icons font
   */
  socialLinks = [
    { icon: 'facebook', url: 'https://facebook.com/eshop', name: 'Facebook' },
    { icon: 'X', url: 'https://twitter.com/eshop', name: 'Twitter' },
    { icon: 'instagram', url: 'https://instagram.com/eshop', name: 'Instagram' },
    { icon: 'linkedin', url: 'https://linkedin.com/company/eshop', name: 'LinkedIn' }
  ];

  constructor(private snackBar: MatSnackBar) {}

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

  /**
   * Subscribe to newsletter
   * TODO: Integrate with backend API when available
   */
  subscribeNewsletter(): void {
    if (!this.newsletterEmail) {
      return;
    }

    this.isSubscribing.set(true);
    this.newsletterMessage.set('');

    // Simulate API call (replace with actual API integration later)
    setTimeout(() => {
      this.isSubscribing.set(false);
      this.newsletterSuccess.set(true);
      this.newsletterMessage.set('Merci! Vous êtes inscrit à notre newsletter.');

      // Show snackbar notification
      this.snackBar.open('Inscription réussie! Consultez votre email.', 'Fermer', {
        duration: 5000,
        horizontalPosition: 'center',
        verticalPosition: 'bottom',
        panelClass: ['success-snackbar']
      });

      // Reset form after 3 seconds
      setTimeout(() => {
        this.newsletterEmail = '';
        this.newsletterMessage.set('');
        this.newsletterSuccess.set(false);
      }, 3000);
    }, 1500);

    // TODO: Replace with actual API call:
    // this.newsletterService.subscribe(this.newsletterEmail).subscribe({
    //   next: () => {
    //     this.isSubscribing.set(false);
    //     this.newsletterSuccess.set(true);
    //     this.newsletterMessage.set('Merci! Vous êtes inscrit à notre newsletter.');
    //   },
    //   error: (error) => {
    //     this.isSubscribing.set(false);
    //     this.newsletterSuccess.set(false);
    //     this.newsletterMessage.set('Erreur lors de l\'inscription. Veuillez réessayer.');
    //   }
    // });
  }
}
