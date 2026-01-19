import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { OrderService } from '../../../core/services/order.service';
import { PaymentService } from '../../../core/services/payment.service';
import { Order } from '../../../core/models/order.model';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-confirmation',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDividerModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './confirmation.html',
  styleUrls: ['./confirmation.scss']
})
export class Confirmation implements OnInit {
  order = signal<Order | null>(null);
  loading = signal(true);
  error = signal<string | null>(null);
  orderId: string | null = null;
  paymentStatus = signal<string | null>(null);
  paymentFailed = signal(false);

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private orderService: OrderService,
    private paymentService: PaymentService
  ) {}

  ngOnInit() {
    this.orderId = this.route.snapshot.paramMap.get('id');

    console.log('Confirmation page loaded for order:', this.orderId);
    console.log('Query params:', this.route.snapshot.queryParamMap.keys);

    // Récupérer tous les query params pour debug
    const allParams: { [key: string]: string } = {};
    this.route.snapshot.queryParamMap.keys.forEach(key => {
      allParams[key] = this.route.snapshot.queryParamMap.get(key) || '';
    });
    console.log('All query parameters:', allParams);

    // Vérifier le statut du paiement depuis les query params (retour Monetbil)
    // Monetbil peut envoyer différents paramètres selon le résultat
    const paymentStatusParam = this.route.snapshot.queryParamMap.get('status');
    const paymentReference = this.route.snapshot.queryParamMap.get('payment_ref');
    const monetbilStatus = this.route.snapshot.queryParamMap.get('monetbil_status');

    // Détecter si le paiement a échoué via différents paramètres possibles
    if ((paymentStatusParam === 'failed' || paymentStatusParam === 'cancelled' ||
         monetbilStatus === 'failed' || monetbilStatus === 'cancelled') &&
        paymentReference) {
      console.log('Payment failed detected via URL parameters');
      // Notifier le backend que le paiement a échoué
      this.paymentService.failPaymentByReference(
        paymentReference,
        'Paiement échoué ou annulé via Monetbil'
      ).subscribe({
        next: () => {
          console.log('Payment marked as failed successfully');
        },
        error: (error) => {
          console.error('Error marking payment as failed:', error);
        }
      });

      this.paymentFailed.set(true);
      this.error.set('Le paiement a échoué ou a été annulé. Votre commande n\'a pas été validée.');
      this.loading.set(false);
      return;
    }

    if (this.orderId) {
      this.loadOrder(this.orderId);
      this.loadPaymentStatus(this.orderId);
    } else {
      this.error.set('No order ID provided');
      this.loading.set(false);
    }
  }

  private loadOrder(orderId: string) {
    this.orderService.getOrderById(orderId).subscribe({
      next: (order) => {
        this.order.set(order);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error loading order:', error);
        this.error.set('Failed to load order details');
        this.loading.set(false);
      }
    });
  }

  private loadPaymentStatus(orderId: string) {
    this.paymentService.getPaymentByOrderId(orderId).subscribe({
      next: (payment) => {
        console.log('Payment status loaded:', payment.status);
        console.log('Payment reference:', payment.paymentReference);
        this.paymentStatus.set(payment.status);

        // Si le paiement a échoué ou est annulé, on affiche l'erreur
        if (payment.status === 'Failed' || payment.status === 'Cancelled') {
          this.paymentFailed.set(true);
          this.error.set(`Le paiement a échoué: ${payment.failureReason || 'Raison inconnue'}`);
          this.order.set(null); // Ne pas afficher la commande
          this.loading.set(false);
        }
        // Si le paiement est toujours en attente ou en cours de traitement
        else if (payment.status === 'Pending' || payment.status === 'Processing') {
          console.log('Payment is still processing, will check again in 5 seconds...');
          console.log('If still processing after 15 seconds, will mark as cancelled');

          // Attendre 5 secondes puis vérifier à nouveau
          setTimeout(() => {
            this.checkPaymentStatusAgain(orderId, payment.paymentReference, 1);
          }, 5000);
        }
      },
      error: (error) => {
        console.error('Error loading payment status:', error);
        // Si le paiement n'existe pas, c'est un problème
        this.paymentFailed.set(true);
        this.error.set('Impossible de charger le statut du paiement');
        this.order.set(null);
        this.loading.set(false);
      }
    });
  }

  private checkPaymentStatusAgain(orderId: string, paymentReference: string, attempt: number) {
    this.paymentService.getPaymentByOrderId(orderId).subscribe({
      next: (payment) => {
        console.log(`Payment status rechecked (attempt ${attempt}):`, payment.status);
        this.paymentStatus.set(payment.status);

        if (payment.status === 'Failed' || payment.status === 'Cancelled') {
          this.paymentFailed.set(true);
          this.error.set(`Le paiement a échoué: ${payment.failureReason || 'Raison inconnue'}`);
          this.order.set(null);
          this.loading.set(false);
        }
        else if (payment.status === 'Completed') {
          // Le paiement a été confirmé entre-temps
          console.log('Payment completed successfully');
          this.paymentStatus.set('Completed');
        }
        else if (payment.status === 'Processing' || payment.status === 'Pending') {
          // Si après 3 tentatives (15 secondes) le paiement est toujours en attente
          // c'est que l'utilisateur a probablement annulé
          if (attempt >= 3) {
            console.warn('Payment still processing after 15 seconds, marking as cancelled');

            // Marquer le paiement comme annulé
            this.paymentService.failPaymentByReference(
              paymentReference,
              'Paiement annulé - L\'utilisateur est revenu sans compléter le paiement'
            ).subscribe({
              next: () => {
                console.log('Payment marked as failed due to timeout');
                this.paymentFailed.set(true);
                this.error.set('Le paiement a été annulé ou a expiré. Votre commande n\'a pas été validée.');
                this.order.set(null);
                this.loading.set(false);
              },
              error: (error) => {
                console.error('Error marking payment as failed:', error);
              }
            });
          } else {
            // Réessayer après 5 secondes
            console.log(`Will check again in 5 seconds (attempt ${attempt + 1}/3)...`);
            setTimeout(() => {
              this.checkPaymentStatusAgain(orderId, paymentReference, attempt + 1);
            }, 5000);
          }
        }
      },
      error: (error) => {
        console.error('Error rechecking payment status:', error);
      }
    });
  }

  getImageUrl(pictureUrl: string | undefined): string {
    if (!pictureUrl) return '/assets/images/placeholder.jpg';
    if (pictureUrl.startsWith('http')) return pictureUrl;
    return `${environment.catalogApiUrl}${pictureUrl}`;
  }

  continueShopping() {
    this.router.navigate(['/catalog']);
  }

  viewOrders() {
    this.router.navigate(['/orders']);
  }

  printOrder() {
    window.print();
  }

  get orderTotal(): number {
    const order = this.order();
    if (!order) return 0;
    return order.subtotal;
  }

  get orderTax(): number {
    const order = this.order();
    if (!order) return 0;
    return order.subtotal * 0.1;
  }
}
