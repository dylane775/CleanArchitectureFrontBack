import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { AdminService } from '../../../core/services/admin/admin.service';
import { AdminStats, OrderSummary } from '../../../core/models/admin/admin.model';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule
  ],
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.scss'
})
export class AdminDashboard implements OnInit {
  stats = signal<AdminStats | null>(null);
  loading = signal(true);
  displayedColumns: string[] = ['orderNumber', 'customer', 'date', 'status', 'total', 'actions'];

  constructor(private adminService: AdminService) {}

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.loading.set(true);

    // Since we don't have a stats endpoint, we'll aggregate data manually
    // In production, you'd have a dedicated endpoint
    this.loading.set(false);
  }

  getStatusClass(status: string): string {
    const statusLower = status.toLowerCase();
    if (statusLower.includes('delivered')) return 'status-delivered';
    if (statusLower.includes('shipped')) return 'status-shipped';
    if (statusLower.includes('processing') || statusLower.includes('submitted')) return 'status-processing';
    if (statusLower.includes('cancelled')) return 'status-cancelled';
    return 'status-pending';
  }
}
