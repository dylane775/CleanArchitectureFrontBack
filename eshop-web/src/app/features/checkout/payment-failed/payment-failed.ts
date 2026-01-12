import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-payment-failed',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule
  ],
  template: `
    <div class="payment-failed-container">
      <mat-card class="failure-card">
        <mat-card-header>
          <div class="failure-icon">
            <mat-icon color="warn">error</mat-icon>
          </div>
          <mat-card-title>Paiement échoué</mat-card-title>
        </mat-card-header>

        <mat-card-content>
          <p class="message">
            Désolé, votre paiement n'a pas pu être traité.
          </p>

          @if (errorMessage()) {
            <p class="error-details">
              Raison : {{ errorMessage() }}
            </p>
          }

          <div class="actions">
            <button mat-raised-button color="primary" (click)="retryPayment()">
              <mat-icon>refresh</mat-icon>
              Réessayer le paiement
            </button>

            <button mat-stroked-button (click)="backToBasket()">
              Retour au panier
            </button>

            <button mat-stroked-button (click)="continueShopping()">
              Continuer mes achats
            </button>
          </div>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .payment-failed-container {
      max-width: 600px;
      margin: 50px auto;
      padding: 20px;
    }

    .failure-card {
      text-align: center;
    }

    .failure-icon {
      display: flex;
      justify-content: center;
      margin-bottom: 20px;

      mat-icon {
        font-size: 80px;
        width: 80px;
        height: 80px;
      }
    }

    mat-card-title {
      font-size: 24px;
      color: #f44336;
      margin-bottom: 20px;
    }

    .message {
      font-size: 16px;
      margin: 20px 0;
      color: #666;
    }

    .error-details {
      background-color: #ffebee;
      padding: 10px;
      border-radius: 4px;
      color: #c62828;
      margin: 15px 0;
    }

    .actions {
      display: flex;
      flex-direction: column;
      gap: 10px;
      margin-top: 30px;

      button {
        width: 100%;
      }
    }
  `]
})
export class PaymentFailed implements OnInit {
  errorMessage = signal<string | null>(null);

  constructor(
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    // Récupérer le message d'erreur depuis les query params si disponible
    this.route.queryParams.subscribe(params => {
      if (params['error']) {
        this.errorMessage.set(params['error']);
      }
    });
  }

  retryPayment(): void {
    this.router.navigate(['/checkout']);
  }

  backToBasket(): void {
    this.router.navigate(['/basket']);
  }

  continueShopping(): void {
    this.router.navigate(['/catalog']);
  }
}
