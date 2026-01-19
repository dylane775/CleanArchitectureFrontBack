import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatStepperModule } from '@angular/material/stepper';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatRadioModule } from '@angular/material/radio';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { BasketService } from '../../core/services/basket.service';
import { OrderService } from '../../core/services/order.service';
import { AuthService } from '../../core/services/auth.service';
import { PaymentService } from '../../core/services/payment.service';
import { Basket, BasketItem } from '../../core/models/basket.model';
import {
  CheckoutRequest,
  CheckoutItem,
  PAYMENT_METHODS,
  PaymentMethod,
  formatAddressAsString
} from '../../core/models/order.model';
import { environment } from '../../../environments/environment';
import { BreadcrumbComponent, BreadcrumbItem } from '../../shared/components/breadcrumb/breadcrumb';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatStepperModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatRadioModule,
    MatCheckboxModule,
    MatCardModule,
    MatIconModule,
    MatDividerModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    BreadcrumbComponent
  ],
  templateUrl: './checkout.html',
  styleUrls: ['./checkout.scss']
})
export class Checkout implements OnInit {
  shippingFormGroup!: FormGroup;
  paymentFormGroup!: FormGroup;

  basket = signal<Basket | null>(null);
  loading = signal(false);
  submitting = signal(false);

  paymentMethods = PAYMENT_METHODS;
  selectedPaymentMethod = signal<PaymentMethod>(PAYMENT_METHODS[0]);

  // Breadcrumb
  breadcrumbItems: BreadcrumbItem[] = [
    { label: 'Accueil', url: '/', icon: 'home' },
    { label: 'Panier', url: '/basket' },
    { label: 'Paiement' }
  ];

  constructor(
    private fb: FormBuilder,
    private basketService: BasketService,
    private orderService: OrderService,
    private paymentService: PaymentService,
    private authService: AuthService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit() {
    this.initForms();
    this.loadBasket();
    this.prefillUserData();
  }

  private initForms() {
    this.shippingFormGroup = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      phone: [''],
      shippingStreet: ['', Validators.required],
      shippingCity: ['', Validators.required],
      shippingState: ['', Validators.required],
      shippingZipCode: ['', Validators.required],
      shippingCountry: ['', Validators.required],
      sameAsBilling: [true],
      billingStreet: [''],
      billingCity: [''],
      billingState: [''],
      billingZipCode: [''],
      billingCountry: ['']
    });

    // Watch for same as billing checkbox changes
    this.shippingFormGroup.get('sameAsBilling')?.valueChanges.subscribe(same => {
      const billingFields = ['billingStreet', 'billingCity', 'billingState', 'billingZipCode', 'billingCountry'];

      if (same) {
        billingFields.forEach(field => {
          this.shippingFormGroup.get(field)?.clearValidators();
          this.shippingFormGroup.get(field)?.updateValueAndValidity();
        });
      } else {
        billingFields.forEach(field => {
          this.shippingFormGroup.get(field)?.setValidators(Validators.required);
          this.shippingFormGroup.get(field)?.updateValueAndValidity();
        });
      }
    });

    this.paymentFormGroup = this.fb.group({
      paymentMethod: ['CreditCard', Validators.required],
      cardName: [''],
      cardNumber: [''],
      cardExpiry: [''],
      cardCvv: ['']
    });

    // Update validators when payment method changes
    this.paymentFormGroup.get('paymentMethod')?.valueChanges.subscribe(method => {
      this.updatePaymentValidators(method);
    });

    // Set initial validators
    this.updatePaymentValidators('CreditCard');
  }

  private updatePaymentValidators(method: string) {
    const cardFields = ['cardName', 'cardNumber', 'cardExpiry', 'cardCvv'];

    if (method === 'CreditCard' || method === 'DebitCard') {
      cardFields.forEach(field => {
        this.paymentFormGroup.get(field)?.setValidators(Validators.required);
        this.paymentFormGroup.get(field)?.updateValueAndValidity();
      });
    } else {
      cardFields.forEach(field => {
        this.paymentFormGroup.get(field)?.clearValidators();
        this.paymentFormGroup.get(field)?.updateValueAndValidity();
      });
    }
  }

  private prefillUserData() {
    const user = this.authService.currentUser();
    if (user) {
      this.shippingFormGroup.patchValue({
        firstName: user.firstName || '',
        lastName: user.lastName || '',
        email: user.email || ''
      });
    }
  }

  private loadBasket() {
    this.loading.set(true);
    this.basketService.getCurrentBasket().subscribe({
      next: (basket) => {
        this.basket.set(basket);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error loading basket:', error);
        this.snackBar.open('Failed to load basket', 'Close', { duration: 3000 });
        this.loading.set(false);
      }
    });
  }

  getImageUrl(pictureUrl: string | undefined): string {
    if (!pictureUrl) return '/assets/images/placeholder.jpg';
    if (pictureUrl.startsWith('http')) return pictureUrl;
    return `${environment.catalogApiUrl}${pictureUrl}`;
  }

  get basketItems(): BasketItem[] {
    return this.basket()?.items || [];
  }

  get subtotal(): number {
    return this.basketItems.reduce((sum, item) => sum + (item.unitPrice * item.quantity), 0);
  }

  get shipping(): number {
    return this.subtotal >= 100 ? 0 : 5;
  }

  get tax(): number {
    return this.subtotal * 0.1;
  }

  get total(): number {
    return this.subtotal + this.shipping + this.tax;
  }

  getBasketTotal(): number {
    return this.total;
  }

  placeOrder() {
    if (!this.shippingFormGroup.valid || !this.paymentFormGroup.valid) {
      this.snackBar.open('Please fill all required fields', 'Close', { duration: 3000 });
      return;
    }

    const basketData = this.basket();
    if (!basketData || basketData.items.length === 0) {
      this.snackBar.open('Your basket is empty', 'Close', { duration: 3000 });
      return;
    }

    this.submitting.set(true);

    const formData = this.shippingFormGroup.value;
    const paymentData = this.paymentFormGroup.value;

    // Format shipping address
    const shippingAddress = formatAddressAsString({
      street: formData.shippingStreet,
      city: formData.shippingCity,
      state: formData.shippingState,
      zipCode: formData.shippingZipCode,
      country: formData.shippingCountry
    });

    // Format billing address
    const billingAddress = formData.sameAsBilling
      ? shippingAddress
      : formatAddressAsString({
          street: formData.billingStreet,
          city: formData.billingCity,
          state: formData.billingState,
          zipCode: formData.billingZipCode,
          country: formData.billingCountry
        });

    // Convert basket items to checkout items
    const items: CheckoutItem[] = basketData.items.map(item => ({
      catalogItemId: item.catalogItemId,
      productName: item.productName,
      unitPrice: item.unitPrice,
      quantity: item.quantity,
      pictureUrl: item.pictureUrl,
      discount: 0
    }));

    const checkoutRequest: CheckoutRequest = {
      customerId: basketData.customerId,
      shippingAddress,
      billingAddress,
      paymentMethod: paymentData.paymentMethod,
      customerEmail: formData.email,
      customerPhone: formData.phone,
      items
    };

    this.orderService.checkout(checkoutRequest).subscribe({
      next: (orderId) => {
        console.log('Order created:', orderId);

        // Initier le paiement via Monetbil
        const paymentMethod = this.paymentFormGroup.get('paymentMethod')?.value;

        if (paymentMethod === 'Monetbil') {
          this.initiateMonetbilPayment(orderId, formData, basketData);
        } else {
          // Pour les autres méthodes de paiement (Cash on Delivery, etc.)
          this.finalizeOrder(basketData.customerId, orderId);
        }
      },
      error: (error) => {
        console.error('Error placing order:', error);
        this.submitting.set(false);
        this.snackBar.open(
          error.error?.message || 'Failed to place order. Please try again.',
          'Close',
          { duration: 5000 }
        );
      }
    });
  }

  /**
   * Initie le paiement via Monetbil
   */
  private initiateMonetbilPayment(orderId: string, formData: any, basketData: Basket) {
    const currentUrl = window.location.origin;

    // ⚠️ IMPORTANT: En développement avec ngrok, utiliser l'URL ngrok pour le webhook
    // En production, utiliser l'URL du backend déployé
    // Pour ngrok: Remplacer par votre URL ngrok actuelle (ex: https://abc123.ngrok-free.app)
    const backendUrl = 'https://08babe70b679.ngrok-free.app'; // TODO: Mettre en variable d'environnement

    const paymentRequest = {
      orderId: orderId,
      customerId: basketData.customerId,
      amount: this.getBasketTotal(),
      currency: 'XAF', // Franc CFA
      paymentProvider: 'Monetbil',
      customerEmail: formData.email,
      customerPhone: formData.phone || '',
      description: `Paiement pour la commande ${orderId}`,
      callbackUrl: `${backendUrl}/api/payments/webhook/monetbil`,
      returnUrl: `${currentUrl}/checkout/confirmation/${orderId}`
    };

    this.paymentService.initiatePayment(paymentRequest).subscribe({
      next: (paymentResponse) => {
        console.log('Payment initiated:', paymentResponse);

        // Rediriger vers la page de paiement Monetbil
        if (paymentResponse.paymentUrl) {
          window.location.href = paymentResponse.paymentUrl;
        } else {
          this.submitting.set(false);
          this.snackBar.open('Payment URL not provided', 'Close', { duration: 5000 });
        }
      },
      error: (error) => {
        console.error('Error initiating payment:', error);
        this.submitting.set(false);
        this.snackBar.open(
          error.error?.message || 'Failed to initiate payment. Please try again.',
          'Close',
          { duration: 5000 }
        );
      }
    });
  }

  /**
   * Finalise la commande (pour les paiements non-Monetbil)
   */
  private finalizeOrder(customerId: string, orderId: string) {
    // Clear the basket after successful order
    this.basketService.deleteBasket(customerId).subscribe({
      next: () => {
        console.log('Basket cleared after checkout');
      },
      error: (error) => {
        console.error('Error clearing basket:', error);
      }
    });

    this.submitting.set(false);
    this.snackBar.open('Order placed successfully!', 'Close', { duration: 3000 });

    // Navigate to order confirmation page
    this.router.navigate(['/checkout/confirmation', orderId]);
  }

  onPaymentMethodChange(method: string) {
    const selectedMethod = this.paymentMethods.find(pm => pm.type === method);
    if (selectedMethod) {
      this.selectedPaymentMethod.set(selectedMethod);
    }
  }
}
