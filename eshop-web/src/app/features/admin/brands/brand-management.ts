import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { AdminService } from '../../../core/services/admin/admin.service';
import { CatalogBrand } from '../../../core/models/admin/admin.model';

@Component({
  selector: 'app-brand-management',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSnackBarModule
  ],
  templateUrl: './brand-management.html',
  styleUrl: './brand-management.scss'
})
export class BrandManagement implements OnInit {
  brands = signal<CatalogBrand[]>([]);
  loading = signal(false);
  displayedColumns = ['brand', 'actions'];

  editingBrand = signal<CatalogBrand | null>(null);
  brandForm!: FormGroup;
  showForm = signal(false);

  constructor(
    private adminService: AdminService,
    private fb: FormBuilder,
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) {
    this.initializeForm();
  }

  ngOnInit(): void {
    this.loadBrands();
  }

  initializeForm(): void {
    this.brandForm = this.fb.group({
      brand: ['', [Validators.required, Validators.minLength(2)]]
    });
  }

  loadBrands(): void {
    this.loading.set(true);
    this.adminService.getAllBrands().subscribe({
      next: (brands) => {
        this.brands.set(brands);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error loading brands:', error);
        this.snackBar.open('Failed to load brands', 'Close', {
          duration: 3000,
          panelClass: ['error-snackbar']
        });
        this.loading.set(false);
      }
    });
  }

  openCreateForm(): void {
    this.editingBrand.set(null);
    this.brandForm.reset();
    this.showForm.set(true);
  }

  openEditForm(brand: CatalogBrand): void {
    this.editingBrand.set(brand);
    this.brandForm.patchValue({ brand: brand.brand });
    this.showForm.set(true);
  }

  cancelForm(): void {
    this.showForm.set(false);
    this.editingBrand.set(null);
    this.brandForm.reset();
  }

  saveBrand(): void {
    if (this.brandForm.invalid) {
      this.brandForm.markAllAsTouched();
      return;
    }

    const brandName = this.brandForm.value.brand;
    this.loading.set(true);

    if (this.editingBrand()) {
      // Update existing brand
      this.adminService.updateBrand(this.editingBrand()!.id, brandName).subscribe({
        next: () => {
          this.snackBar.open('Brand updated successfully', '✓', {
            duration: 2500,
            horizontalPosition: 'end',
            verticalPosition: 'top',
            panelClass: ['success-snackbar']
          });
          this.loadBrands();
          this.cancelForm();
        },
        error: (error) => {
          console.error('Error updating brand:', error);
          this.snackBar.open('Failed to update brand', 'Close', {
            duration: 3000,
            panelClass: ['error-snackbar']
          });
          this.loading.set(false);
        }
      });
    } else {
      // Create new brand
      this.adminService.createBrand(brandName).subscribe({
        next: () => {
          this.snackBar.open('Brand created successfully', '✓', {
            duration: 2500,
            horizontalPosition: 'end',
            verticalPosition: 'top',
            panelClass: ['success-snackbar']
          });
          this.loadBrands();
          this.cancelForm();
        },
        error: (error) => {
          console.error('Error creating brand:', error);
          this.snackBar.open('Failed to create brand', 'Close', {
            duration: 3000,
            panelClass: ['error-snackbar']
          });
          this.loading.set(false);
        }
      });
    }
  }

  deleteBrand(brand: CatalogBrand): void {
    if (!confirm(`Are you sure you want to delete the brand "${brand.brand}"?`)) {
      return;
    }

    this.loading.set(true);
    this.adminService.deleteBrand(brand.id).subscribe({
      next: () => {
        this.snackBar.open('Brand deleted successfully', '✓', {
          duration: 2500,
          horizontalPosition: 'end',
          verticalPosition: 'top',
          panelClass: ['success-snackbar']
        });
        this.loadBrands();
      },
      error: (error) => {
        console.error('Error deleting brand:', error);
        this.snackBar.open('Failed to delete brand', 'Close', {
          duration: 3000,
          panelClass: ['error-snackbar']
        });
        this.loading.set(false);
      }
    });
  }
}
