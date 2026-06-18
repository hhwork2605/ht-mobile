import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { MultiSelectModule } from 'primeng/multiselect';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { UsersService } from '../../core/users.service';
import { UserRow } from '../../core/user.models';

@Component({
  selector: 'app-users-list',
  standalone: true,
  imports: [FormsModule, TableModule, TagModule, ButtonModule, DialogModule, InputTextModule,
    PasswordModule, MultiSelectModule, ToastModule],
  providers: [MessageService],
  template: `
    <p-toast />
    <div class="head">
      <h1 class="h1">Người dùng & quyền</h1>
      <p-button label="Thêm người dùng" icon="pi pi-plus" (onClick)="openCreate()" />
    </div>

    <div class="card">
      <p-table [value]="rows()" [loading]="loading()" styleClass="p-datatable-sm" [tableStyle]="{ 'min-width': '52rem' }">
        <ng-template pTemplate="header">
          <tr><th>Email</th><th>Họ tên</th><th>Vai trò</th><th class="ta-c">Trạng thái</th><th></th></tr>
        </ng-template>
        <ng-template pTemplate="body" let-u>
          <tr>
            <td class="strong">{{ u.email }}</td>
            <td class="muted">{{ u.fullName }}</td>
            <td>
              @for (r of u.roles; track r) {
                <p-tag [value]="r" [severity]="r === 'Admin' ? 'info' : 'secondary'" styleClass="mr-1" />
              }
            </td>
            <td class="ta-c"><p-tag [value]="u.lockedOut ? 'Đã khoá' : 'Hoạt động'" [severity]="u.lockedOut ? 'danger' : 'success'" /></td>
            <td class="ta-r nowrap">
              <p-button label="Quyền" [text]="true" size="small" (onClick)="openRoles(u)" />
              @if (u.lockedOut) {
                <p-button label="Mở khoá" [text]="true" size="small" (onClick)="toggleLock(u)" />
              } @else {
                <p-button label="Khoá" [text]="true" size="small" severity="danger" (onClick)="toggleLock(u)" />
              }
            </td>
          </tr>
        </ng-template>
        <ng-template pTemplate="emptymessage"><tr><td colspan="5" class="empty">Chưa có người dùng.</td></tr></ng-template>
      </p-table>
    </div>

    <!-- Dialog tạo người dùng -->
    <p-dialog header="Thêm người dùng" [(visible)]="createVisible" [modal]="true" [style]="{ width: '420px' }">
      <div class="form">
        <label>Email *</label>
        <input pInputText [(ngModel)]="cEmail" class="w-full" type="email" />
        <label>Họ tên</label>
        <input pInputText [(ngModel)]="cFullName" class="w-full" />
        <label>Mật khẩu *</label>
        <p-password [(ngModel)]="cPassword" [feedback]="false" [toggleMask]="true" styleClass="w-full" inputStyleClass="w-full" />
        <label>Vai trò</label>
        <p-multiSelect [options]="allRoles" [(ngModel)]="cRoles" placeholder="Chọn vai trò" styleClass="w-full" appendTo="body" />
      </div>
      <ng-template pTemplate="footer">
        <p-button label="Huỷ" severity="secondary" [outlined]="true" (onClick)="createVisible = false" />
        <p-button label="Tạo" icon="pi pi-check" [loading]="saving()" (onClick)="create()" />
      </ng-template>
    </p-dialog>

    <!-- Dialog sửa quyền -->
    <p-dialog header="Phân quyền" [(visible)]="rolesVisible" [modal]="true" [style]="{ width: '380px' }">
      <p class="muted sm">Tài khoản: <b>{{ editing?.email }}</b></p>
      <p-multiSelect [options]="allRoles" [(ngModel)]="editRoles" placeholder="Chọn vai trò" styleClass="w-full" appendTo="body" />
      <ng-template pTemplate="footer">
        <p-button label="Huỷ" severity="secondary" [outlined]="true" (onClick)="rolesVisible = false" />
        <p-button label="Lưu" icon="pi pi-check" [loading]="saving()" (onClick)="saveRoles()" />
      </ng-template>
    </p-dialog>
  `,
  styles: [`
    .head { display: flex; align-items: center; justify-content: space-between; margin-bottom: 16px; }
    .h1 { font-size: 20px; font-weight: 700; margin: 0; }
    .card { background: #fff; border: 1px solid var(--ht-line); border-radius: 14px; overflow: hidden; }
    .strong { font-weight: 600; } .muted { color: var(--ht-ink5); } .sm { font-size: 13px; }
    .ta-c { text-align: center; } .ta-r { text-align: right; } .nowrap { white-space: nowrap; }
    .mr-1 { margin-right: 4px; }
    .empty { text-align: center; padding: 28px; color: var(--ht-ink4); }
    .form { display: flex; flex-direction: column; gap: 6px; }
    .form label { font-size: 12.5px; font-weight: 600; color: var(--ht-ink5); margin-top: 8px; }
    .w-full { width: 100%; }
  `],
})
export class UsersListComponent implements OnInit {
  rows = signal<UserRow[]>([]);
  loading = signal(false);
  saving = signal(false);
  allRoles: string[] = [];

  createVisible = false;
  cEmail = ''; cFullName = ''; cPassword = ''; cRoles: string[] = ['Admin'];

  rolesVisible = false;
  editing: UserRow | null = null;
  editRoles: string[] = [];

  constructor(private api: UsersService, private toast: MessageService) {}

  ngOnInit(): void {
    this.reload();
    this.api.roles().subscribe((r) => (this.allRoles = r));
  }

  reload(): void {
    this.loading.set(true);
    this.api.list().subscribe({ next: (r) => { this.rows.set(r); this.loading.set(false); }, error: () => this.loading.set(false) });
  }

  openCreate(): void { this.cEmail = ''; this.cFullName = ''; this.cPassword = ''; this.cRoles = ['Admin']; this.createVisible = true; }

  create(): void {
    this.saving.set(true);
    this.api.create({ email: this.cEmail, fullName: this.cFullName, password: this.cPassword, roles: this.cRoles }).subscribe({
      next: () => { this.saving.set(false); this.createVisible = false; this.toast.add({ severity: 'success', summary: 'Đã tạo người dùng' }); this.reload(); },
      error: (e) => { this.saving.set(false); this.toast.add({ severity: 'error', summary: 'Lỗi', detail: e?.error?.message ?? 'Tạo thất bại.' }); },
    });
  }

  openRoles(u: UserRow): void { this.editing = u; this.editRoles = [...u.roles]; this.rolesVisible = true; }

  saveRoles(): void {
    if (!this.editing) return;
    this.saving.set(true);
    this.api.setRoles(this.editing.id, this.editRoles).subscribe({
      next: () => { this.saving.set(false); this.rolesVisible = false; this.toast.add({ severity: 'success', summary: 'Đã cập nhật quyền' }); this.reload(); },
      error: (e) => { this.saving.set(false); this.toast.add({ severity: 'error', summary: 'Lỗi', detail: e?.error?.message ?? 'Cập nhật thất bại.' }); },
    });
  }

  toggleLock(u: UserRow): void {
    const op = u.lockedOut ? this.api.unlock(u.id) : this.api.lock(u.id);
    op.subscribe({
      next: () => { this.toast.add({ severity: 'success', summary: u.lockedOut ? 'Đã mở khoá' : 'Đã khoá' }); this.reload(); },
      error: (e) => this.toast.add({ severity: 'error', summary: 'Lỗi', detail: e?.error?.message ?? 'Thao tác thất bại.' }),
    });
  }
}
