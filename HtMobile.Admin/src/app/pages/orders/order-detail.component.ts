import { Component, OnInit, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { TagModule } from 'primeng/tag';
import { SelectModule } from 'primeng/select';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { OrdersService } from '../../core/orders.service';
import {
  AdminOrderDetailDto, OrderStatus, orderStatusLabel, orderStatusSeverity,
} from '../../core/order.models';

@Component({
  selector: 'app-order-detail',
  standalone: true,
  imports: [DatePipe, FormsModule, RouterLink, TagModule, SelectModule, ButtonModule, ToastModule],
  providers: [MessageService],
  template: `
    <p-toast />
    <a routerLink="/orders" class="back"><i class="pi pi-arrow-left"></i> Tất cả đơn hàng</a>

    @if (order(); as o) {
      <div class="head">
        <div>
          <span class="code">#{{ o.code }}</span>
          <span class="date">{{ o.createdAt | date:'dd/MM/yyyy HH:mm' }}</span>
        </div>
        <p-tag [value]="label(o.status)" [severity]="sev(o.status)" />
      </div>

      <div class="grid">
        <div class="col">
          <section class="card">
            <h2 class="h2">Thông tin</h2>
            <div class="info">
              <div><div class="k">Khách hàng</div><div class="v">{{ o.customerId ? 'User #' + o.customerId : 'Khách vãng lai' }}</div></div>
              <div><div class="k">Thanh toán</div><div class="v">{{ o.paymentMethod }}</div></div>
              <div class="span2"><div class="k">Nhận hàng</div><div class="v">{{ o.shipmentAddress || '—' }}</div></div>
            </div>
          </section>

          <section class="card">
            <h2 class="h2">Sản phẩm</h2>
            @for (it of o.items; track it) {
              <div class="line">
                <div><span class="strong">{{ it.productName }}</span><span class="muted"> {{ it.variantText }} · SL {{ it.quantity }}</span></div>
                <span class="strong">{{ vnd(it.lineTotal) }}</span>
              </div>
            }
            <div class="total"><span>Tổng tiền</span><span class="grand">{{ vnd(o.total) }}</span></div>
          </section>
        </div>

        <div class="col side">
          <section class="card">
            <h2 class="h2">Cập nhật trạng thái</h2>
            @if (o.allowedNext.length === 0) {
              <p class="muted sm">Đơn ở trạng thái cuối ({{ label(o.status) }}) — không thể đổi.</p>
            } @else {
              <p-select [options]="nextOptions(o)" [(ngModel)]="nextStatus" optionLabel="label" optionValue="value"
                        placeholder="Chọn trạng thái" styleClass="w-full mb-3" appendTo="body" />
              <p-button label="Cập nhật" icon="pi pi-check" [loading]="saving()" [disabled]="nextStatus === null"
                        styleClass="w-full" (onClick)="change(o)" />
            }
          </section>
        </div>
      </div>
    }
  `,
  styles: [`
    .back { display: inline-flex; align-items: center; gap: 6px; color: var(--ht-ink5); text-decoration: none; font-size: 13px; margin-bottom: 12px; }
    .head { display: flex; align-items: center; justify-content: space-between; margin-bottom: 16px; }
    .code { font-family: ui-monospace, monospace; font-size: 19px; font-weight: 700; }
    .date { margin-left: 12px; font-size: 13px; color: var(--ht-ink5); }
    .grid { display: flex; gap: 16px; align-items: flex-start; flex-wrap: wrap; }
    .col { flex: 1; min-width: 320px; display: flex; flex-direction: column; gap: 16px; }
    .side { flex: 0 0 300px; }
    .card { background: #fff; border: 1px solid var(--ht-line); border-radius: 14px; padding: 20px; }
    .h2 { font-size: 15px; font-weight: 700; margin: 0 0 14px; }
    .info { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; font-size: 13px; }
    .span2 { grid-column: 1 / -1; }
    .k { color: var(--ht-ink5); } .v { font-weight: 500; }
    .line { display: flex; justify-content: space-between; padding: 9px 0; border-bottom: 1px solid var(--ht-line); font-size: 13px; }
    .strong { font-weight: 600; } .muted { color: var(--ht-ink5); } .sm { font-size: 13px; }
    .total { display: flex; justify-content: space-between; align-items: baseline; padding-top: 12px; margin-top: 4px; font-weight: 700; }
    .grand { font-size: 20px; }
    .w-full { width: 100%; } .mb-3 { margin-bottom: 14px; }
  `],
})
export class OrderDetailComponent implements OnInit {
  order = signal<AdminOrderDetailDto | null>(null);
  saving = signal(false);
  nextStatus: OrderStatus | null = null;
  private id = 0;

  constructor(private api: OrdersService, private route: ActivatedRoute, private toast: MessageService) {}

  ngOnInit(): void {
    this.id = Number(this.route.snapshot.paramMap.get('id'));
    this.load();
  }

  load(): void {
    this.api.get(this.id).subscribe((o) => { this.order.set(o); this.nextStatus = null; });
  }

  nextOptions(o: AdminOrderDetailDto) {
    return o.allowedNext.map((s) => ({ label: orderStatusLabel(s), value: s }));
  }

  change(o: AdminOrderDetailDto): void {
    if (this.nextStatus === null) return;
    this.saving.set(true);
    this.api.changeStatus(o.id, this.nextStatus).subscribe({
      next: (updated) => { this.saving.set(false); this.order.set(updated); this.nextStatus = null; this.toast.add({ severity: 'success', summary: 'Đã cập nhật trạng thái' }); },
      error: (e) => { this.saving.set(false); this.toast.add({ severity: 'error', summary: 'Lỗi', detail: e?.error?.message ?? 'Không đổi được trạng thái.' }); },
    });
  }

  label = orderStatusLabel;
  sev = orderStatusSeverity;
  vnd(n: number): string { return n.toLocaleString('vi-VN') + '₫'; }
}
