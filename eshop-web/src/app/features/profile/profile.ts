import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTabsModule } from '@angular/material/tabs';
import { MatDividerModule } from '@angular/material/divider';
import { MatChipsModule } from '@angular/material/chips';
import { AuthService } from '../../core/services/auth.service';
import { UserService } from '../../core/services/user.service';
import { User } from '../../core/models/auth.model';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatInputModule,
    MatFormFieldModule,
    MatSnackBarModule,
    MatTabsModule,
    MatDividerModule,
    MatChipsModule
  ],
  templateUrl: './profile.html',
  styleUrl: './profile.scss',
})
export class Profile implements OnInit {
  user = signal<User | null>(null);
  profileForm!: FormGroup;
  passwordForm!: FormGroup;
  editMode = signal(false);
  loading = signal(false);

  constructor(
    private authService: AuthService,
    private userService: UserService,
    private fb: FormBuilder,
    private router: Router,
    private snackBar: MatSnackBar
  ) {
    this.initializeForms();
  }

  ngOnInit(): void {
    this.loadUserProfile();
  }

  private initializeForms(): void {
    this.profileForm = this.fb.group({
      firstName: ['', [Validators.required, Validators.minLength(2)]],
      lastName: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]]
    });

    this.passwordForm = this.fb.group({
      currentPassword: ['', [Validators.required, Validators.minLength(6)]],
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required, Validators.minLength(6)]]
    }, { validators: this.passwordMatchValidator });

    // Disable forms by default
    this.profileForm.disable();
  }

  private passwordMatchValidator(group: FormGroup): { [key: string]: boolean } | null {
    const newPassword = group.get('newPassword')?.value;
    const confirmPassword = group.get('confirmPassword')?.value;
    return newPassword === confirmPassword ? null : { passwordMismatch: true };
  }

  loadUserProfile(): void {
    const currentUser = this.authService.currentUser();

    if (!currentUser) {
      this.router.navigate(['/auth/login']);
      return;
    }

    this.user.set(currentUser);
    this.profileForm.patchValue({
      firstName: currentUser.firstName || '',
      lastName: currentUser.lastName || '',
      email: currentUser.email || ''
    });
  }

  toggleEditMode(): void {
    const newEditMode = !this.editMode();
    this.editMode.set(newEditMode);

    if (newEditMode) {
      this.profileForm.enable();
    } else {
      this.profileForm.disable();
      this.loadUserProfile(); // Reset form to original values
    }
  }

  saveProfile(): void {
    if (this.profileForm.invalid) {
      this.snackBar.open('Please fill all required fields correctly', 'Close', { duration: 3000 });
      return;
    }

    const currentUser = this.user();
    if (!currentUser) return;

    this.loading.set(true);

    this.userService.updateProfile(currentUser.id, this.profileForm.value).subscribe({
      next: (updatedUser) => {
        // Update local storage and signal
        localStorage.setItem('user', JSON.stringify(updatedUser));
        this.authService.currentUser.set(updatedUser);
        this.user.set(updatedUser);

        this.loading.set(false);
        this.editMode.set(false);
        this.profileForm.disable();
        this.snackBar.open('Profile updated successfully!', 'Close', { duration: 3000 });
      },
      error: (err: any) => {
        console.error('Error updating profile:', err);
        this.loading.set(false);
        this.snackBar.open('Error updating profile. Please try again.', 'Close', { duration: 3000 });
      }
    });
  }

  changePassword(): void {
    if (this.passwordForm.invalid) {
      if (this.passwordForm.hasError('passwordMismatch')) {
        this.snackBar.open('Passwords do not match', 'Close', { duration: 3000 });
      } else {
        this.snackBar.open('Please fill all password fields correctly', 'Close', { duration: 3000 });
      }
      return;
    }

    const currentUser = this.user();
    if (!currentUser) return;

    this.loading.set(true);

    this.userService.changePassword(currentUser.id, {
      currentPassword: this.passwordForm.value.currentPassword,
      newPassword: this.passwordForm.value.newPassword
    }).subscribe({
      next: () => {
        this.loading.set(false);
        this.passwordForm.reset();
        this.snackBar.open('Password changed successfully!', 'Close', { duration: 3000 });
      },
      error: (err: any) => {
        console.error('Error changing password:', err);
        this.loading.set(false);
        this.snackBar.open('Error changing password. Please check your current password.', 'Close', { duration: 3000 });
      }
    });
  }

  deleteAccount(): void {
    if (confirm('Are you sure you want to delete your account? This action cannot be undone.')) {
      const currentUser = this.user();
      if (!currentUser) return;

      this.loading.set(true);

      this.userService.deleteAccount(currentUser.id).subscribe({
        next: () => {
          this.authService.logout();
          this.router.navigate(['/auth/login']);
          this.snackBar.open('Account deleted successfully', 'Close', { duration: 3000 });
        },
        error: (err: any) => {
          console.error('Error deleting account:', err);
          this.loading.set(false);
          this.snackBar.open('Error deleting account. Please try again.', 'Close', { duration: 3000 });
        }
      });
    }
  }

  getUserInitials(): string {
    const user = this.user();
    if (!user) return '?';

    const firstInitial = user.firstName?.charAt(0) || '';
    const lastInitial = user.lastName?.charAt(0) || '';
    return (firstInitial + lastInitial).toUpperCase() || user.email?.charAt(0).toUpperCase() || '?';
  }
}
