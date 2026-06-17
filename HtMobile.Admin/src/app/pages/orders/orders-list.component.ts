import { Component, OnInit, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { OrdersService } from '../../core/orders.service';
import {
  AdminOrderRow, OrderStatus, ORDER_STATUSES, orderStatusLabel, orderStatusSeverity,
} from '../../core/order.models';

@Component({
  selector: 'app-orders-list',
  standalone: true,
  imports: [DatePipe, RouterLink, TableModule, TagModule, ButtonModule],
  template: `
    <h1 class="h1">Đơn hàng</h1>

    <div class="card">
      <div class="tabs">
        <button class="tab" [class.active]="filter === null" (click)="select(null)">
          Tất cả <span class="badge">{{ totalCount() }}</span>
        </button>
        @for (s of statuses; track s) {
          <button class="tab" [class.active]="filter === s" (click)="select(s)">
            {{ label(s) }} <span class="badge">{{ count(s) }}</span>
          </button>
        }
      </div>

      <p-table [value]="rows()" [loading]="loading()" [paginator]="rows().length > 15" [rows]="15"
               styleClass="p-datatable-sm" [tableStyle]="{ 'min-width': '64rem' }">
        <ng-template pTemplate="header">
          <tr>
            <th>Mã đơn</th><th>Khách hàng</th><th>Sản phẩm</th>
            <th class="ta-r">Tổng tiền</th><th class="ta-c">Thanh toán</th>
            <th class="ta-c">Trạng thái</th><th>Ngày</th>
          </tr>
        </ng-template>
        <ng-template pTemplate="body" let-o>
          <tr>
            <td><a class="code" [routerLink]="['/orders', o.id]">#{{ o.code }}</a></td>
            <td>{{ o.recipient }}</td>
            <td class="muted">{{ o.itemsSummary }}</td>
            <td class="ta-r strong">{{ vnd(o.total) }}</td>
            <td class="ta-c muted">{{ o.paymentMethod }}</td>
            <td class="ta-c"><p-tag [value]="label(o.status)" [severity]="sev(o.status)" /></td>
            <td class="muted">{{ o.createdAt | date:'dd/MM HH:mm' }}</td>
          </tr>
        </ng-template>
        <ng-template pTemplate="emptymessage">
          <tr><td colspan="7" class="empty">Không có đơn hàng nào.</td></tr>
        </ng-template>
      </p-table>
    </div>
  `,
  styles: [`
    .h1 { font-size: 20px; font-weight: 700; margin: 0 0 16px; }
    .card { background: #fff; border: 1px solid var(--ht-line); border-radius: 14px; overflow: hidden; }
    .tabs { display: flex; flex-wrap: wrap; gap: 4px; padding: 10px 12px; border-bottom: 1px solid var(--ht-line); }
    .tab { display: inline-flex; align-items: center; gap: 6px; border: 0; background: transparent; cursor: pointer;
      font-size: 13px; font-weight: 600; color: var(--ht-ink5); padding: 7px 12px; border-radius: 9px; }
    .tab:hover { background: var(--ht-surf2); }
    .tab.active { background: var(--ht-brand); color: #fff; }
    .badge { font-size: 11px; font-weight: 700; background: rgba(0,0,0,.06); border-radius: 8px; padding: 0 6px; }
    .tab.active .badge { background: rgba(255,255,255,.25); }
    .code { font-family: ui-monospace, monospace; font-weight: 700; color: var(--ht-brand); text-decoration: none; }
    .muted { color: var(--ht-ink5); } .strong { font-weight: 700; }
    .ta-c { text-align: center; } .ta-r { text-align: right; }
    .empty { text-align: center; padding: 28px; color: var(--ht-ink4); }
  `],
})
export class OrdersListComponent implements OnInit {
  rows = signal<AdminOrderRow[]>([]);
  loading = signal(false);
  totalCount = signal(0);
  counts: Record<string, number> = {};
  filter: OrderStatus | null = null;
  statuses = ORDER_STATUSES;

  constructor(private api: OrdersService) {}

  ngOnInit(): void { this.reload(); }

  select(s: OrderStatus | null): void { this.filter = s; this.reload(); }

  reload(): void {
    this.loading.set(true);
    this.api.list(this.filter).subscribe({
      next: (dto) => {
        this.rows.set(dto.orders);
        this.counts = dto.counts ?? {};
        this.totalCount.set(dto.totalCount);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  // API serialize key dict theo TÊN enum ("Pending"); vẫn phòng trường hợp key số.
  count(s: OrderStatus): number { return this.counts[OrderStatus[s]] ?? this.counts[String(s)] ?? 0; }
  label = orderStatusLabel;
  sev = orderStatusSeverity;
  vnd(n: number): string { return n.toLocaleString('vi-VN') + '₫'; }
}
