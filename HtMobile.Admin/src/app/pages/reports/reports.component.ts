import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SelectModule } from 'primeng/select';
import { ChartModule } from 'primeng/chart';
import { TableModule } from 'primeng/table';
import { ReportsService } from '../../core/reports.service';
import { SalesReport } from '../../core/report.models';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [FormsModule, SelectModule, ChartModule, TableModule],
  template: `
    <div class="head">
      <h1 class="h1">Báo cáo bán hàng</h1>
      <p-select [options]="presets" [(ngModel)]="days" optionLabel="label" optionValue="value"
                styleClass="preset" appendTo="body" (onChange)="load()" />
    </div>

    @if (report(); as r) {
      <div class="kpis">
        <div class="kpi"><div class="kpi-label">Tổng doanh thu</div><div class="kpi-val">{{ vnd(r.revenueTotal) }}</div></div>
        <div class="kpi"><div class="kpi-label">Số đơn hàng</div><div class="kpi-val">{{ r.ordersCount }}</div></div>
        <div class="kpi"><div class="kpi-label">Giá trị TB/đơn</div><div class="kpi-val">{{ vnd(r.avgOrderValue) }}</div></div>
        <div class="kpi"><div class="kpi-label">Tỷ lệ huỷ/hoàn</div><div class="kpi-val">{{ r.cancelRate }}%</div></div>
      </div>

      <div class="card">
        <h2 class="h2">Doanh thu theo ngày</h2>
        <p-chart type="line" [data]="chartData" [options]="chartOptions" height="280px" />
      </div>

      <div class="card">
        <h2 class="h2">Sản phẩm bán chạy</h2>
        <p-table [value]="r.topProducts" styleClass="p-datatable-sm">
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
          <ng-template pTemplate="emptymessage"><tr><td colspan="4" class="empty">Chưa có dữ liệu bán trong kỳ.</td></tr></ng-template>
        </p-table>
      </div>
    }
  `,
  styles: [`
    .head { display: flex; align-items: center; justify-content: space-between; margin-bottom: 16px; }
    .h1 { font-size: 20px; font-weight: 700; margin: 0; }
    :host ::ng-deep .preset { min-width: 180px; }
    .kpis { display: grid; grid-template-columns: repeat(4, 1fr); gap: 14px; margin-bottom: 16px; }
    .kpi { background: #fff; border: 1px solid var(--ht-line); border-radius: 14px; padding: 16px; }
    .kpi-label { font-size: 12px; color: var(--ht-ink5); }
    .kpi-val { font-size: 20px; font-weight: 700; margin-top: 4px; }
    .card { background: #fff; border: 1px solid var(--ht-line); border-radius: 14px; padding: 18px; margin-bottom: 16px; }
    .h2 { font-size: 15px; font-weight: 700; margin: 0 0 14px; }
    .strong { font-weight: 600; } .muted { color: var(--ht-ink5); }
    .ta-c { text-align: center; } .ta-r { text-align: right; }
    .empty { text-align: center; padding: 20px; color: var(--ht-ink4); }
    @media(max-width:1100px){ .kpis { grid-template-columns: repeat(2,1fr); } }
  `],
})
export class ReportsComponent implements OnInit {
  report = signal<SalesReport | null>(null);
  days = 30;
  presets = [
    { label: '7 ngày qua', value: 7 },
    { label: '30 ngày qua', value: 30 },
    { label: '90 ngày qua', value: 90 },
  ];
  chartData: any = {};
  chartOptions: any = {
    maintainAspectRatio: false,
    plugins: { legend: { display: false } },
    scales: { y: { beginAtZero: true, ticks: { callback: (v: number) => (v / 1_000_000).toLocaleString('vi-VN') + 'M' } } },
  };

  constructor(private api: ReportsService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    const to = new Date();
    const from = new Date(); from.setDate(to.getDate() - (this.days - 1));
    this.api.sales(this.fmt(from), this.fmt(to)).subscribe((r) => {
      this.report.set(r);
      this.chartData = {
        labels: r.byDay.map((d) => this.dayLabel(d.date)),
        datasets: [{
          label: 'Doanh thu', data: r.byDay.map((d) => d.revenue),
          borderColor: '#0070F4', backgroundColor: 'rgba(0,112,244,.12)', fill: true, tension: 0.35, pointRadius: 2,
        }],
      };
    });
  }

  private fmt(d: Date): string {
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
  }
  private dayLabel(iso: string): string { const d = new Date(iso); return `${d.getDate()}/${d.getMonth() + 1}`; }
  vnd(n: number): string { return n.toLocaleString('vi-VN') + '₫'; }
}
