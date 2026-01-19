import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface PaymentInitiateRequest {
  orderId: string;
  customerId: string;
  amount: number;
  currency: string;
  paymentProvider: string;
  customerEmail: string;
  customerPhone?: string;
  description: string;
  callbackUrl: string;
  returnUrl: string;
}

export interface PaymentInitiateResponse {
  paymentId: string;
  paymentReference: string;
  status: string;
  paymentUrl: string;
  qrCodeUrl?: string;
}

export interface Payment {
  id: string;
  orderId: string;
  customerId: string;
  amount: number;
  currency: string;
  status: string;
  provider: string;
  transactionId?: string;
  paymentReference: string;
  customerEmail: string;
  customerPhone?: string;
  description?: string;
  failureReason?: string;
  completedAt?: Date;
  failedAt?: Date;
  refundedAmount: number;
  refundedAt?: Date;
  createdAt: Date;
}

@Injectable({
  providedIn: 'root'
})
export class PaymentService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.paymentApiUrl}/payments`;

  /**
   * Initie un nouveau paiement
   */
  initiatePayment(request: PaymentInitiateRequest): Observable<PaymentInitiateResponse> {
    return this.http.post<PaymentInitiateResponse>(this.apiUrl, request);
  }

  /**
   * Récupère un paiement par son ID
   */
  getPaymentById(paymentId: string): Observable<Payment> {
    return this.http.get<Payment>(`${this.apiUrl}/${paymentId}`);
  }

  /**
   * Récupère le paiement d'une commande
   */
  getPaymentByOrderId(orderId: string): Observable<Payment> {
    return this.http.get<Payment>(`${this.apiUrl}/order/${orderId}`);
  }

  /**
   * Récupère un paiement par sa référence
   */
  getPaymentByReference(reference: string): Observable<Payment> {
    return this.http.get<Payment>(`${this.apiUrl}/reference/${reference}`);
  }

  /**
   * Récupère tous les paiements d'un client
   */
  getPaymentsByCustomerId(customerId: string): Observable<Payment[]> {
    return this.http.get<Payment[]>(`${this.apiUrl}/customer/${customerId}`);
  }

  /**
   * Annule un paiement
   */
  cancelPayment(paymentId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${paymentId}/cancel`, {});
  }

  /**
   * Marque un paiement comme échoué via la référence
   */
  failPaymentByReference(reference: string, reason: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/reference/${reference}/fail`, {
      failureReason: reason
    });
  }
}
