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
import { CatalogType } from '../../../core/models/admin/admin.model';

@Component({
  selector: 'app-type-management',
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
  templateUrl: './type-management.html',
  styleUrl: './type-management.scss'
})
export class TypeManagement implements OnInit {
  types = signal<CatalogType[]>([]);
  loading = signal(false);
  displayedColumns = ['type', 'actions'];

  editingType = signal<CatalogType | null>(null);
  typeForm!: FormGroup;
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
    this.loadTypes();
  }

  initializeForm(): void {
    this.typeForm = this.fb.group({
      type: ['', [Validators.required, Validators.minLength(2)]]
    });
  }

  loadTypes(): void {
    this.loading.set(true);
    this.adminService.getAllTypes().subscribe({
      next: (types) => {
        this.types.set(types);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error loading types:', error);
        this.snackBar.open('Failed to load categories', 'Close', {
          duration: 3000,
          panelClass: ['error-snackbar']
        });
        this.loading.set(false);
      }
    });
  }

  openCreateForm(): void {
    this.editingType.set(null);
    this.typeForm.reset();
    this.showForm.set(true);
  }

  openEditForm(type: CatalogType): void {
    this.editingType.set(type);
    this.typeForm.patchValue({ type: type.type });
    this.showForm.set(true);
  }

  cancelForm(): void {
    this.showForm.set(false);
    this.editingType.set(null);
    this.typeForm.reset();
  }

  saveType(): void {
    if (this.typeForm.invalid) {
      this.typeForm.markAllAsTouched();
      return;
    }

    const typeName = this.typeForm.value.type;
    this.loading.set(true);

    if (this.editingType()) {
      // Update existing type
      this.adminService.updateType(this.editingType()!.id, typeName).subscribe({
        next: () => {
          this.snackBar.open('Category updated successfully', '✓', {
            duration: 2500,
            horizontalPosition: 'end',
            verticalPosition: 'top',
            panelClass: ['success-snackbar']
          });
          this.loadTypes();
          this.cancelForm();
        },
        error: (error) => {
          console.error('Error updating type:', error);
          this.snackBar.open('Failed to update category', 'Close', {
            duration: 3000,
            panelClass: ['error-snackbar']
          });
          this.loading.set(false);
        }
      });
    } else {
      // Create new type
      this.adminService.createType(typeName).subscribe({
        next: () => {
          this.snackBar.open('Category created successfully', '✓', {
            duration: 2500,
            horizontalPosition: 'end',
            verticalPosition: 'top',
            panelClass: ['success-snackbar']
          });
          this.loadTypes();
          this.cancelForm();
        },
        error: (error) => {
          console.error('Error creating type:', error);
          this.snackBar.open('Failed to create category', 'Close', {
            duration: 3000,
            panelClass: ['error-snackbar']
          });
          this.loading.set(false);
        }
      });
    }
  }

  deleteType(type: CatalogType): void {
    if (!confirm(`Are you sure you want to delete the category "${type.type}"?`)) {
      return;
    }

    this.loading.set(true);
    this.adminService.deleteType(type.id).subscribe({
      next: () => {
        this.snackBar.open('Category deleted successfully', '✓', {
          duration: 2500,
          horizontalPosition: 'end',
          verticalPosition: 'top',
          panelClass: ['success-snackbar']
        });
        this.loadTypes();
      },
      error: (error) => {
        console.error('Error deleting type:', error);
        this.snackBar.open('Failed to delete category', 'Close', {
          duration: 3000,
          panelClass: ['error-snackbar']
        });
        this.loading.set(false);
      }
    });
  }
}
