import { Component, Inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../../core/services/admin/admin.service';
import { User, Role } from '../../../core/models/admin/admin.model';

@Component({
  selector: 'role-assignment-dialog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatCheckboxModule,
    MatSnackBarModule
  ],
  template: `
    <div class="role-dialog">
      <h2 mat-dialog-title>
        <mat-icon>admin_panel_settings</mat-icon>
        Manage User Roles
      </h2>

      <mat-dialog-content>
        <div class="user-info">
          <div class="user-avatar">
            {{ getUserInitial() }}
          </div>
          <div class="user-details">
            <h3>{{ getUserDisplayName() }}</h3>
            <p>{{ data.user.email }}</p>
          </div>
        </div>

        <div class="roles-section">
          <h4>Available Roles</h4>
          @if (loading()) {
            <p class="loading-text">Loading roles...</p>
          } @else {
            <div class="roles-list">
              @for (role of availableRoles(); track role.id) {
                <div class="role-item">
                  <mat-checkbox
                    [checked]="isRoleAssigned(role.name)"
                    (change)="toggleRole(role.name, $event.checked)"
                    [disabled]="saving()"
                  >
                    <div class="role-info">
                      <span class="role-name">{{ role.name }}</span>
                      <span class="role-description">{{ getRoleDescription(role.name) }}</span>
                    </div>
                  </mat-checkbox>
                </div>
              }
            </div>
          }
        </div>
      </mat-dialog-content>

      <mat-dialog-actions align="end">
        <button mat-button (click)="cancel()" [disabled]="saving()">
          Cancel
        </button>
        <button mat-raised-button color="primary" (click)="save()" [disabled]="saving() || !hasChanges()">
          <mat-icon>save</mat-icon>
          {{ saving() ? 'Saving...' : 'Save Changes' }}
        </button>
      </mat-dialog-actions>
    </div>
  `,
  styles: [`
    .role-dialog {
      min-width: 450px;

      h2 {
        display: flex;
        align-items: center;
        gap: 12px;
        margin: 0;

        mat-icon {
          color: #FFD700;
        }
      }

      mat-dialog-content {
        padding: 24px 0;
      }

      .user-info {
        display: flex;
        align-items: center;
        gap: 16px;
        padding: 20px;
        background: linear-gradient(135deg, #f8f9fa 0%, #ffffff 100%);
        border-radius: 12px;
        margin-bottom: 24px;

        .user-avatar {
          width: 56px;
          height: 56px;
          border-radius: 50%;
          background: linear-gradient(135deg, #87CEEB 0%, #4682B4 100%);
          color: white;
          display: flex;
          align-items: center;
          justify-content: center;
          font-size: 1.5rem;
          font-weight: 600;
          flex-shrink: 0;
        }

        .user-details {
          h3 {
            margin: 0 0 4px 0;
            font-size: 1.2rem;
            font-weight: 600;
            color: #2C3E50;
          }

          p {
            margin: 0;
            color: #7F8C8D;
            font-size: 0.95rem;
          }
        }
      }

      .roles-section {
        h4 {
          margin: 0 0 16px 0;
          font-size: 1rem;
          font-weight: 600;
          color: #2C3E50;
        }

        .loading-text {
          text-align: center;
          color: #7F8C8D;
          padding: 20px;
        }

        .roles-list {
          display: flex;
          flex-direction: column;
          gap: 12px;
        }

        .role-item {
          padding: 12px;
          border: 2px solid #e0e0e0;
          border-radius: 8px;
          transition: all 0.3s ease;

          &:hover {
            border-color: #FFD700;
            background-color: rgba(255, 215, 0, 0.05);
          }

          mat-checkbox {
            width: 100%;
          }

          .role-info {
            display: flex;
            flex-direction: column;
            gap: 4px;

            .role-name {
              font-weight: 600;
              color: #2C3E50;
            }

            .role-description {
              font-size: 0.85rem;
              color: #7F8C8D;
            }
          }
        }
      }

      mat-dialog-actions {
        padding: 16px 0 0 0;
        margin: 0;
        border-top: 1px solid #e0e0e0;

        button {
          mat-icon {
            margin-right: 8px;
          }
        }
      }
    }
  `]
})
export class RoleAssignmentDialog implements OnInit {
  availableRoles = signal<Role[]>([]);
  userRoles = signal<string[]>([]);
  originalRoles: string[] = [];
  loading = signal(false);
  saving = signal(false);

  constructor(
    public dialogRef: MatDialogRef<RoleAssignmentDialog>,
    @Inject(MAT_DIALOG_DATA) public data: { user: User },
    private adminService: AdminService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.loadRoles();
    this.loadUserRoles();
  }

  loadRoles(): void {
    this.loading.set(true);
    this.adminService.getAllRoles().subscribe({
      next: (roles) => {
        this.availableRoles.set(roles);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error loading roles:', error);
        this.snackBar.open('Failed to load roles', 'Close', { duration: 3000 });
        this.loading.set(false);
      }
    });
  }

  loadUserRoles(): void {
    this.adminService.getUserRoles(this.data.user.id).subscribe({
      next: (roles) => {
        this.userRoles.set(roles);
        this.originalRoles = [...roles];
      },
      error: (error) => {
        console.error('Error loading user roles:', error);
      }
    });
  }

  getUserInitial(): string {
    return this.getUserDisplayName().charAt(0).toUpperCase();
  }

  getUserDisplayName(): string {
    const user = this.data.user;
    if (user.firstName && user.lastName) {
      return `${user.firstName} ${user.lastName}`;
    }
    if (user.firstName) {
      return user.firstName;
    }
    return user.email;
  }

  isRoleAssigned(roleName: string): boolean {
    return this.userRoles().includes(roleName);
  }

  toggleRole(roleName: string, checked: boolean): void {
    const current = this.userRoles();
    if (checked) {
      this.userRoles.set([...current, roleName]);
    } else {
      this.userRoles.set(current.filter(r => r !== roleName));
    }
  }

  hasChanges(): boolean {
    const current = this.userRoles().sort();
    const original = this.originalRoles.sort();
    return JSON.stringify(current) !== JSON.stringify(original);
  }

  getRoleDescription(roleName: string): string {
    const descriptions: { [key: string]: string } = {
      'Admin': 'Full system access and management',
      'Customer': 'Standard customer access',
      'Manager': 'Product and order management'
    };
    return descriptions[roleName] || 'No description available';
  }

  async save(): Promise<void> {
    this.saving.set(true);

    const added = this.userRoles().filter(r => !this.originalRoles.includes(r));
    const removed = this.originalRoles.filter(r => !this.userRoles().includes(r));

    try {
      // Add new roles
      for (const role of added) {
        await this.adminService.assignRole({
          userId: this.data.user.id,
          roleName: role
        }).toPromise();
      }

      // Remove roles
      for (const role of removed) {
        await this.adminService.removeRole({
          userId: this.data.user.id,
          roleName: role
        }).toPromise();
      }

      this.snackBar.open('Roles updated successfully', '✓', {
        duration: 2500,
        horizontalPosition: 'end',
        verticalPosition: 'top',
        panelClass: ['success-snackbar']
      });

      this.dialogRef.close(true);
    } catch (error) {
      console.error('Error updating roles:', error);
      this.snackBar.open('Failed to update roles', 'Close', {
        duration: 3000,
        panelClass: ['error-snackbar']
      });
      this.saving.set(false);
    }
  }

  cancel(): void {
    this.dialogRef.close(false);
  }
}
