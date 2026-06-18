import { Component, OnInit, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ChartModule } from 'primeng/chart';
import { DashboardService } from '../../core/dashboard.service';
import { DashboardStats } from '../../core/dashboard.models';
import { ORDER_STATUSES, OrderStatus, orderStatusLabel, orderStatusSeverity } from '../../core/order.models';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [DatePipe, RouterLink, TableModule, TagModule, ChartModule],
  template: `
    <h1 class="h1">Bảng điều khiển</h1>

    @if (stats(); as s) {
      <div class="kpis">
        <div class="kpi">
          <div class="kpi-ic" style="background:#E5F1FE;color:#0070F4"><i class="pi pi-dollar"></i></div>
          <div><div class="kpi-label">Doanh thu hôm nay</div><div class="kpi-val">{{ vnd(s.revenueToday) }}</div>
            <div class="kpi-sub">Tổng: {{ vnd(s.revenueTotal) }}</div></div>
        </div>
        <div class="kpi">
          <div class="kpi-ic" style="background:#E6F8EC;color:#00B63E"><i class="pi pi-shopping-cart"></i></div>
          <div><div class="kpi-label">Đơn hàng</div><div class="kpi-val">{{ s.ordersTotal }}</div>
            <div class="kpi-sub">Hôm nay: {{ s.ordersToday }}</div></div>
        </div>
        <div class="kpi">
          <div class="kpi-ic" style="background:#F3E9FE;color:#8A38F5"><i class="pi pi-users"></i></div>
          <div><div class="kpi-label">Khách đã mua</div><div class="kpi-val">{{ s.customersCount }}</div>
            <div class="kpi-sub">khách có tài khoản</div></div>
        </div>
        <div class="kpi">
          <div class="kpi-ic" style="background:#FFF3E0;color:#FF8800"><i class="pi pi-box"></i></div>
          <div><div class="kpi-label">Sản phẩm</div><div class="kpi-val">{{ s.productsCount }}</div>
            <div class="kpi-sub">model đang bán</div></div>
        </div>
      </div>

      <div class="grid">
        <section class="card">
          <h2 class="h2">Doanh thu 14 ngày</h2>
          <p-chart type="line" [data]="chartData" [options]="chartOptions" height="240px" />
        </section>

        <section class="card side">
          <div class="lowstock-head">
            <h2 class="h2">Tồn kho thấp</h2>
            <p-tag [value]="s.lowStockCount + ''" [severity]="s.lowStockCount ? 'danger' : 'success'" />
          </div>
          @if (s.lowStock.length === 0) {
            <p class="muted sm">Không có biến thể nào dưới ngưỡng ({{ s.lowStockThreshold }}).</p>
          } @else {
            @for (l of s.lowStock; track l.productId) {
              <div class="lrow">
                <div class="lname"><span class="strong">{{ l.name }}</span> <span class="muted">{{ l.variantLabel }}</span></div>
                <p-tag [value]="l.totalQuantity + ''" [severity]="l.totalQuantity === 0 ? 'danger' : 'warn'" />
              </div>
            }
            <a class="link" routerLink="/inventory">Cập nhật tồn kho →</a>
          }
        </section>
      </div>

      <div class="grid">
        <section class="card">
          <h2 class="h2">Sản phẩm bán chạy (30 ngày)</h2>
          <p-table [value]="s.topProducts" styleClass="p-datatable-sm">
            <ng-template pTemplate="header">
              <tr><th style="width:3rem">#</th><th>Sản phẩm</th><th class="ta-c">SL bán</th><th class="ta-r">Doanh thu</th></tr>
            </ng-template>
            <ng-template pTemplate="body" let-p let-i="rowIndex">
              <tr>
                <td class="muted">{{ i + 1 }}</td>
                <td><span class="strong">{{ p.name }}</span> <span class="muted">{{ p.variantLabel }}</span></td>
                <td class="ta-c strong">{{ p.quantitySold }}</td>
                <td class="ta-r">{{ vnd(p.revenue) }}</td>
              </tr>
            </ng-template>
            <ng-template pTemplate="emptymessage"><tr><td colspan="4" class="empty">Chưa có dữ liệu bán.</td></tr></ng-template>
          </p-table>
        </section>

        <section class="card side">
          <h2 class="h2">Đơn theo trạng thái</h2>
          @for (st of statuses; track st) {
            <div class="srow">
              <p-tag [value]="label(st)" [severity]="sev(st)" />
              <span class="snum">{{ count(s, st) }}</span>
            </div>
          }
        </section>
      </div>

      <div class="grid">
        <section class="card">
          <h2 class="h2">Đơn hàng gần đây</h2>
          <p-table [value]="s.recentOrders" styleClass="p-datatable-sm">
            <ng-template pTemplate="header">
              <tr><th>Mã đơn</th><th>Nhận hàng</th><th class="ta-r">Tổng tiền</th><th class="ta-c">Trạng thái</th><th>Ngày</th></tr>
            </ng-template>
            <ng-template pTemplate="body" let-o>
              <tr>
                <td><a class="code" [routerLink]="['/orders', o.id]">#{{ o.code }}</a></td>
                <td class="muted">{{ o.recipient }}</td>
                <td class="ta-r strong">{{ vnd(o.total) }}</td>
                <td class="ta-c"><p-tag [value]="label(o.status)" [severity]="sev(o.status)" /></td>
                <td class="muted">{{ o.createdAt | date:'dd/MM HH:mm' }}</td>
              </tr>
            </ng-template>
            <ng-template pTemplate="emptymessage"><tr><td colspan="5" class="empty">Chưa có đơn hàng.</td></tr></ng-template>
          </p-table>
        </section>
      </div>
    }
  `,
  styles: [`
    .h1 { font-size: 20px; font-weight: 700; margin: 0 0 16px; }
    .kpis { display: grid; grid-template-columns: repeat(4, 1fr); gap: 14px; margin-bottom: 16px; }
    .kpi { background: #fff; border: 1px solid var(--ht-line); border-radius: 14px; padding: 16px; display: flex; gap: 12px; align-items: center; }
    .kpi-ic { width: 44px; height: 44px; border-radius: 12px; display: flex; align-items: center; justify-content: center; font-size: 18px; flex: none; }
    .kpi-label { font-size: 12px; color: var(--ht-ink5); }
    .kpi-val { font-size: 20px; font-weight: 700; line-height: 1.2; }
    .kpi-sub { font-size: 11px; color: var(--ht-ink4); }
    .grid { display: flex; gap: 16px; align-items: flex-start; flex-wrap: wrap; }
    .card { background: #fff; border: 1px solid var(--ht-line); border-radius: 14px; padding: 18px; flex: 1; min-width: 320px; }
    .side { flex: 0 0 280px; }
    .h2 { font-size: 15px; font-weight: 700; margin: 0 0 12px; }
    .code { font-family: ui-monospace, monospace; font-weight: 700; color: var(--ht-brand); text-decoration: none; }
    .muted { color: var(--ht-ink5); } .strong { font-weight: 700; }
    .ta-c { text-align: center; } .ta-r { text-align: right; }
    .empty { text-align: center; padding: 20px; color: var(--ht-ink4); }
    .srow { display: flex; align-items: center; justify-content: space-between; padding: 8px 0; border-bottom: 1px solid var(--ht-line); }
    .snum { font-weight: 700; }
    .lowstock-head { display: flex; align-items: center; justify-content: space-between; margin-bottom: 8px; }
    .lowstock-head .h2 { margin: 0; }
    .lrow { display: flex; align-items: center; justify-content: space-between; gap: 8px; padding: 7px 0; border-bottom: 1px solid var(--ht-line); }
    .lname { font-size: 12.5px; min-width: 0; }
    .sm { font-size: 13px; } .link { display: inline-block; margin-top: 10px; font-size: 13px; color: var(--ht-brand); text-decoration: none; }
    @media(max-width:1100px){ .kpis { grid-template-columns: repeat(2,1fr); } }
  `],
})
export class DashboardComponent implements OnInit {
  stats = signal<DashboardStats | null>(null);
  statuses = ORDER_STATUSES;
  chartData: any = {};
  chartOptions: any = {
    maintainAspectRatio: false,
    plugins: { legend: { display: false } },
    scales: { y: { beginAtZero: true, ticks: { callback: (v: number) => (v / 1_000_000).toLocaleString('vi-VN') + 'M' } } },
  };

  constructor(private api: DashboardService) {}

  ngOnInit(): void {
    this.api.get().subscribe((s) => {
      this.stats.set(s);
      this.chartData = {
        labels: s.revenueByDay.map((d) => this.dayLabel(d.date)),
        datasets: [{
          label: 'Doanh thu', data: s.revenueByDay.map((d) => d.revenue),
          borderColor: '#0070F4', backgroundColor: 'rgba(0,112,244,.12)', fill: true, tension: 0.35, pointRadius: 2,
        }],
      };
    });
  }

  private dayLabel(iso: string): string { const d = new Date(iso); return `${d.getDate()}/${d.getMonth() + 1}`; }

  count(s: DashboardStats, st: OrderStatus): number { return s.statusCounts[OrderStatus[st]] ?? s.statusCounts[String(st)] ?? 0; }
  label = orderStatusLabel;
  sev = orderStatusSeverity;
  vnd(n: number): string { return n.toLocaleString('vi-VN') + '₫'; }
}
