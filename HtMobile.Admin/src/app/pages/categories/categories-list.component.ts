import { Component, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService, MessageService } from 'primeng/api';
import { CategoriesService } from '../../core/categories.service';
import { AdminCategoryRow } from '../../core/category.models';

@Component({
  selector: 'app-categories-list',
  standalone: true,
  imports: [RouterLink, TableModule, ButtonModule, ToastModule, ConfirmDialogModule],
  providers: [ConfirmationService, MessageService],
  template: `
    <p-toast />
    <p-confirmdialog />
    <div class="head">
      <h1 class="h1">Danh mục</h1>
      <p-button label="Thêm danh mục" icon="pi pi-plus" routerLink="/categories/new" />
    </div>

    <div class="card">
      <p-table [value]="rows()" [loading]="loading()" styleClass="p-datatable-sm" [tableStyle]="{ 'min-width': '52rem' }">
        <ng-template pTemplate="header">
          <tr>
            <th>Tên</th><th>Slug</th><th>Danh mục cha</th>
            <th class="ta-c">Thứ tự</th><th class="ta-c">Sản phẩm</th><th class="ta-c">Con</th><th></th>
          </tr>
        </ng-template>
        <ng-template pTemplate="body" let-c>
          <tr>
            <td class="strong">{{ c.name }}</td>
            <td class="muted mono">{{ c.slug }}</td>
            <td class="muted">{{ c.parentName || '—' }}</td>
            <td class="ta-c">{{ c.sortOrder }}</td>
            <td class="ta-c">{{ c.productCount }}</td>
            <td class="ta-c">{{ c.childCount }}</td>
            <td class="ta-r nowrap">
              <p-button label="Sửa" [text]="true" size="small" [routerLink]="['/categories', c.id]" />
              <p-button icon="pi pi-trash" [text]="true" size="small" severity="danger" (onClick)="confirmDelete(c)" />
            </td>
          </tr>
        </ng-template>
        <ng-template pTemplate="emptymessage"><tr><td colspan="7" class="empty">Chưa có danh mục.</td></tr></ng-template>
      </p-table>
    </div>
  `,
  styles: [`
    .head { display: flex; align-items: center; justify-content: space-between; margin-bottom: 16px; }
    .h1 { font-size: 20px; font-weight: 700; margin: 0; }
    .card { background: #fff; border: 1px solid var(--ht-line); border-radius: 14px; overflow: hidden; }
    .strong { font-weight: 600; } .muted { color: var(--ht-ink5); } .mono { font-family: ui-monospace, monospace; font-size: 12.5px; }
    .ta-c { text-align: center; } .ta-r { text-align: right; } .nowrap { white-space: nowrap; }
    .empty { text-align: center; padding: 28px; color: var(--ht-ink4); }
  `],
})
export class CategoriesListComponent implements OnInit {
  rows = signal<AdminCategoryRow[]>([]);
  loading = signal(false);

  constructor(private api: CategoriesService, private confirm: ConfirmationService, private toast: MessageService) {}

  ngOnInit(): void { this.reload(); }

  reload(): void {
    this.loading.set(true);
    this.api.list().subscribe({ next: (r) => { this.rows.set(r); this.loading.set(false); }, error: () => this.loading.set(false) });
  }

  confirmDelete(c: AdminCategoryRow): void {
    this.confirm.confirm({
      header: 'Xoá danh mục',
      message: `Xoá "${c.name}"?`,
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Xoá', rejectLabel: 'Huỷ', acceptButtonStyleClass: 'p-button-danger',
      accept: () => this.api.delete(c.id).subscribe({
        next: () => { this.toast.add({ severity: 'success', summary: 'Đã xoá' }); this.reload(); },
        error: (e) => this.toast.add({ severity: 'error', summary: 'Không xoá được', detail: e?.error?.message ?? '' }),
      }),
    });
  }
}
