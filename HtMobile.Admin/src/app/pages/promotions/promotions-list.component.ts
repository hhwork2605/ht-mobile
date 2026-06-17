import { Component, OnInit, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService, MessageService } from 'primeng/api';
import { PromotionsService } from '../../core/promotions.service';
import { AdminPromotionRow, promotionTypeLabel, promotionValueText } from '../../core/promotion.models';

@Component({
  selector: 'app-promotions-list',
  standalone: true,
  imports: [DatePipe, RouterLink, TableModule, TagModule, ButtonModule, ToastModule, ConfirmDialogModule],
  providers: [ConfirmationService, MessageService],
  template: `
    <p-toast />
    <p-confirmdialog />
    <div class="head">
      <h1 class="h1">Khuyến mãi</h1>
      <p-button label="Thêm khuyến mãi" icon="pi pi-plus" routerLink="/promotions/new" />
    </div>

    <div class="card">
      <p-table [value]="rows()" [loading]="loading()" styleClass="p-datatable-sm" [tableStyle]="{ 'min-width': '60rem' }">
        <ng-template pTemplate="header">
          <tr>
            <th>Tên</th><th>Loại</th><th class="ta-r">Giá trị</th>
            <th>Bắt đầu</th><th>Kết thúc</th><th class="ta-c">Trạng thái</th><th></th>
          </tr>
        </ng-template>
        <ng-template pTemplate="body" let-p>
          <tr>
            <td class="strong">{{ p.name }}</td>
            <td><p-tag [value]="typeLabel(p.type)" severity="secondary" /></td>
            <td class="ta-r strong">{{ valueText(p) }}</td>
            <td class="muted">{{ p.startsAt | date:'dd/MM/yy HH:mm' }}</td>
            <td class="muted">{{ p.endsAt | date:'dd/MM/yy HH:mm' }}</td>
            <td class="ta-c"><p-tag [value]="p.isActiveNow ? 'Đang chạy' : 'Ngừng'" [severity]="p.isActiveNow ? 'success' : 'secondary'" /></td>
            <td class="ta-r nowrap">
              <p-button label="Sửa" [text]="true" size="small" [routerLink]="['/promotions', p.id]" />
              <p-button icon="pi pi-trash" [text]="true" size="small" severity="danger" (onClick)="confirmDelete(p)" />
            </td>
          </tr>
        </ng-template>
        <ng-template pTemplate="emptymessage"><tr><td colspan="7" class="empty">Chưa có khuyến mãi nào.</td></tr></ng-template>
      </p-table>
    </div>
  `,
  styles: [`
    .head { display: flex; align-items: center; justify-content: space-between; margin-bottom: 16px; }
    .h1 { font-size: 20px; font-weight: 700; margin: 0; }
    .card { background: #fff; border: 1px solid var(--ht-line); border-radius: 14px; overflow: hidden; }
    .strong { font-weight: 600; } .muted { color: var(--ht-ink5); }
    .ta-c { text-align: center; } .ta-r { text-align: right; } .nowrap { white-space: nowrap; }
    .empty { text-align: center; padding: 28px; color: var(--ht-ink4); }
  `],
})
export class PromotionsListComponent implements OnInit {
  rows = signal<AdminPromotionRow[]>([]);
  loading = signal(false);

  constructor(private api: PromotionsService, private confirm: ConfirmationService, private toast: MessageService) {}

  ngOnInit(): void { this.reload(); }

  reload(): void {
    this.loading.set(true);
    this.api.list().subscribe({ next: (r) => { this.rows.set(r); this.loading.set(false); }, error: () => this.loading.set(false) });
  }

  confirmDelete(p: AdminPromotionRow): void {
    this.confirm.confirm({
      header: 'Xoá khuyến mãi',
      message: `Xoá "${p.name}"? Thao tác không hoàn tác.`,
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Xoá', rejectLabel: 'Huỷ', acceptButtonStyleClass: 'p-button-danger',
      accept: () => this.api.delete(p.id).subscribe({
        next: () => { this.toast.add({ severity: 'success', summary: 'Đã xoá' }); this.reload(); },
        error: () => this.toast.add({ severity: 'error', summary: 'Lỗi', detail: 'Không xoá được.' }),
      }),
    });
  }

  typeLabel = promotionTypeLabel;
  valueText = promotionValueText;
}
