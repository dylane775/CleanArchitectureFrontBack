import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { CatalogService } from '../../../core/services/catalog.service';
import { AdminService } from '../../../core/services/admin/admin.service';
import { CatalogBrand, CatalogType } from '../../../core/models/admin/admin.model';

@Component({
  selector: 'app-product-form',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSelectModule,
    MatIconModule,
    MatSnackBarModule
  ],
  templateUrl: './product-form.html',
  styleUrl: './product-form.scss'
})
export class ProductForm implements OnInit {
  productForm!: FormGroup;
  brands = signal<CatalogBrand[]>([]);
  types = signal<CatalogType[]>([]);
  loading = signal(false);
  isEditMode = signal(false);
  productId = signal<string | null>(null);
  imagePreview = signal<string | null>(null);
  uploadingImage = signal(false);
  selectedFile: File | null = null;

  // Specifications management
  specifications = signal<Array<{ key: string; value: string }>>([]);
  newSpecKey = signal('');
  newSpecValue = signal('');

  constructor(
    private fb: FormBuilder,
    private adminService: AdminService,
    private catalogService: CatalogService,
    private router: Router,
    private route: ActivatedRoute,
    private snackBar: MatSnackBar
  ) {
    this.initializeForm();
  }

  ngOnInit(): void {
    this.loadBrandsAndTypes();

    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEditMode.set(true);
      this.productId.set(id);
      this.loadProduct(id);
    }
  }

  initializeForm(): void {
    this.productForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(3)]],
      description: ['', [Validators.required, Validators.minLength(10)]],
      price: [0, [Validators.required, Validators.min(0)]],
      pictureFileName: ['', Validators.required],
      catalogTypeId: ['', Validators.required],
      catalogBrandId: ['', Validators.required],
      availableStock: [0, [Validators.required, Validators.min(0)]],
      restockThreshold: [5, [Validators.required, Validators.min(0)]],
      maxStockThreshold: [100, [Validators.required, Validators.min(0)]]
    });
  }

  loadBrandsAndTypes(): void {
    this.adminService.getAllBrands().subscribe({
      next: (brands) => this.brands.set(brands),
      error: (error) => console.error('Error loading brands:', error)
    });

    this.adminService.getAllTypes().subscribe({
      next: (types) => this.types.set(types),
      error: (error) => console.error('Error loading types:', error)
    });
  }

  loadProduct(id: string): void {
    this.loading.set(true);
    this.catalogService.getCatalogItemById(id).subscribe({
      next: (product) => {
        this.productForm.patchValue({
          name: product.name,
          description: product.description,
          price: product.price,
          pictureFileName: product.pictureUri?.split('/').pop() || '',
          catalogTypeId: product.catalogTypeId,
          catalogBrandId: product.catalogBrandId,
          availableStock: product.availableStock,
          restockThreshold: 5,
          maxStockThreshold: 100
        });

        // Load specifications if they exist
        if (product.specifications) {
          const specsArray = Object.entries(product.specifications).map(([key, value]) => ({
            key,
            value
          }));
          this.specifications.set(specsArray);
        }

        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error loading product:', error);
        this.snackBar.open('Failed to load product', 'Close', {
          duration: 3000,
          panelClass: ['error-snackbar']
        });
        this.loading.set(false);
      }
    });
  }

  onSubmit(): void {
    if (this.productForm.invalid) {
      this.productForm.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    const formValue = { ...this.productForm.value };

    // Convert specifications array to object if there are specifications
    if (this.specifications().length > 0) {
      const specsObject: { [key: string]: string } = {};
      this.specifications().forEach(spec => {
        if (spec.key && spec.value) {
          specsObject[spec.key] = spec.value;
        }
      });
      formValue.specifications = specsObject;
    }

    if (this.isEditMode() && this.productId()) {
      this.adminService.updateProduct(this.productId()!, formValue).subscribe({
        next: () => {
          this.snackBar.open('Product updated successfully', '✓', {
            duration: 2500,
            horizontalPosition: 'end',
            verticalPosition: 'top',
            panelClass: ['success-snackbar']
          });
          this.router.navigate(['/admin/products']);
        },
        error: (error) => {
          console.error('Error updating product:', error);
          this.snackBar.open('Failed to update product', 'Close', {
            duration: 3000,
            panelClass: ['error-snackbar']
          });
          this.loading.set(false);
        }
      });
    } else {
      this.adminService.createProduct(formValue).subscribe({
        next: () => {
          this.snackBar.open('Product created successfully', '✓', {
            duration: 2500,
            horizontalPosition: 'end',
            verticalPosition: 'top',
            panelClass: ['success-snackbar']
          });
          this.router.navigate(['/admin/products']);
        },
        error: (error) => {
          console.error('Error creating product:', error);
          this.snackBar.open('Failed to create product', 'Close', {
            duration: 3000,
            panelClass: ['error-snackbar']
          });
          this.loading.set(false);
        }
      });
    }
  }

  cancel(): void {
    this.router.navigate(['/admin/products']);
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      const file = input.files[0];

      // Validate file type
      if (!file.type.startsWith('image/')) {
        this.snackBar.open('Please select an image file', 'Close', {
          duration: 3000,
          panelClass: ['error-snackbar']
        });
        return;
      }

      // Validate file size (5MB max)
      if (file.size > 5 * 1024 * 1024) {
        this.snackBar.open('Image size must be less than 5MB', 'Close', {
          duration: 3000,
          panelClass: ['error-snackbar']
        });
        return;
      }

      this.selectedFile = file;

      // Show preview
      const reader = new FileReader();
      reader.onload = (e) => {
        this.imagePreview.set(e.target?.result as string);
      };
      reader.readAsDataURL(file);
    }
  }

  uploadImage(): void {
    if (!this.selectedFile) {
      this.snackBar.open('Please select an image first', 'Close', {
        duration: 3000,
        panelClass: ['error-snackbar']
      });
      return;
    }

    this.uploadingImage.set(true);

    this.adminService.uploadProductImage(this.selectedFile).subscribe({
      next: (response) => {
        this.productForm.patchValue({
          pictureFileName: response.fileName
        });
        this.snackBar.open('Image uploaded successfully', '✓', {
          duration: 2500,
          horizontalPosition: 'end',
          verticalPosition: 'top',
          panelClass: ['success-snackbar']
        });
        this.uploadingImage.set(false);
      },
      error: (error) => {
        console.error('Error uploading image:', error);
        this.snackBar.open('Failed to upload image', 'Close', {
          duration: 3000,
          panelClass: ['error-snackbar']
        });
        this.uploadingImage.set(false);
      }
    });
  }

  removeImage(): void {
    this.selectedFile = null;
    this.imagePreview.set(null);
    this.productForm.patchValue({
      pictureFileName: ''
    });
  }

  getImageUrl(fileName: string): string {
    return this.adminService.getProductImageUrl(fileName);
  }

  // Specifications management methods
  addSpecification(): void {
    const key = this.newSpecKey().trim();
    const value = this.newSpecValue().trim();

    if (!key || !value) {
      this.snackBar.open('Please enter both key and value', 'Close', {
        duration: 2000,
        panelClass: ['error-snackbar']
      });
      return;
    }

    // Check if key already exists
    if (this.specifications().some(spec => spec.key === key)) {
      this.snackBar.open('Specification key already exists', 'Close', {
        duration: 2000,
        panelClass: ['error-snackbar']
      });
      return;
    }

    this.specifications.update(specs => [...specs, { key, value }]);
    this.newSpecKey.set('');
    this.newSpecValue.set('');
  }

  removeSpecification(index: number): void {
    this.specifications.update(specs => specs.filter((_, i) => i !== index));
  }

  updateSpecificationKey(index: number, newKey: string): void {
    this.specifications.update(specs => {
      const updated = [...specs];
      updated[index] = { ...updated[index], key: newKey };
      return updated;
    });
  }

  updateSpecificationValue(index: number, newValue: string): void {
    this.specifications.update(specs => {
      const updated = [...specs];
      updated[index] = { ...updated[index], value: newValue };
      return updated;
    });
  }
}
