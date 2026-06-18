import { Component, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TagModule } from 'primeng/tag';
import { SeoService } from '../../core/seo.service';
import { SeoOverview, SeoIssueItem } from '../../core/seo.models';

@Component({
  selector: 'app-seo',
  standalone: true,
  imports: [RouterLink, TagModule],
  template: `
    <h1 class="h1">SEO & Kỹ thuật</h1>

    @if (data(); as d) {
      <div class="kpis">
        <div class="kpi"><div class="kpi-label">URL trong sitemap</div><div class="kpi-val">{{ d.sitemapUrlCount }}</div></div>
        <div class="kpi"><div class="kpi-label">Sản phẩm (model)</div><div class="kpi-val">{{ d.productModelCount }}</div></div>
        <div class="kpi"><div class="kpi-label">Danh mục</div><div class="kpi-val">{{ d.categoryCount }}</div></div>
        <div class="kpi"><div class="kpi-label">Tổng cảnh báo SEO</div><div class="kpi-val" [class.ok]="d.totalIssues === 0">{{ d.totalIssues }}</div></div>
      </div>

      <p class="note">
        <i class="pi pi-info-circle"></i>
        <code>/sitemap.xml</code> và <code>/robots.txt</code> do storefront (HtMobile.Web) phục vụ, server-render.
        Bảng dưới kiểm tra dữ liệu SEO catalog — bấm để sửa nhanh.
      </p>

      <div class="groups">
        @for (g of d.groups; track g.key) {
          <section class="card">
            <div class="g-head">
              <h2 class="h2">{{ g.title }}</h2>
              @if (g.count === 0) {
                <p-tag value="Tốt" severity="success" />
              } @else {
                <p-tag [value]="g.count + ' mục'" [severity]="sev(g.severity)" />
              }
            </div>
            @if (g.count === 0) {
              <p class="ok-line"><i class="pi pi-check-circle"></i> Không có vấn đề.</p>
            } @else {
              <ul class="items">
                @for (it of g.items; track it.id) {
                  <li><a [routerLink]="link(it)">{{ it.name }}</a></li>
                }
              </ul>
              @if (g.count > g.items.length) {
                <p class="more">… và {{ g.count - g.items.length }} mục khác.</p>
              }
            }
          </section>
        }
      </div>
    }
  `,
  styles: [`
    .h1 { font-size: 20px; font-weight: 700; margin: 0 0 16px; }
    .kpis { display: grid; grid-template-columns: repeat(4, 1fr); gap: 14px; margin-bottom: 14px; }
    .kpi { background: #fff; border: 1px solid var(--ht-line); border-radius: 14px; padding: 16px; }
    .kpi-label { font-size: 12px; color: var(--ht-ink5); }
    .kpi-val { font-size: 20px; font-weight: 700; margin-top: 4px; }
    .kpi-val.ok { color: var(--ht-brand); }
    .note { display: flex; gap: 8px; align-items: center; font-size: 12.5px; color: var(--ht-ink5); margin: 0 0 16px; }
    .note code { background: var(--ht-surf2); padding: 1px 6px; border-radius: 6px; font-size: 12px; }
    .groups { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; }
    .card { background: #fff; border: 1px solid var(--ht-line); border-radius: 14px; padding: 18px; }
    .g-head { display: flex; align-items: center; justify-content: space-between; margin-bottom: 10px; }
    .h2 { font-size: 14.5px; font-weight: 700; margin: 0; }
    .ok-line { color: #00a63a; font-size: 13px; margin: 0; display: flex; gap: 6px; align-items: center; }
    .items { margin: 0; padding-left: 18px; }
    .items li { padding: 3px 0; font-size: 13px; }
    .items a { color: var(--ht-brand); text-decoration: none; }
    .items a:hover { text-decoration: underline; }
    .more { font-size: 12px; color: var(--ht-ink4); margin: 8px 0 0; }
    @media(max-width:1100px){ .kpis { grid-template-columns: repeat(2,1fr); } .groups { grid-template-columns: 1fr; } }
  `],
})
export class SeoComponent implements OnInit {
  data = signal<SeoOverview | null>(null);

  constructor(private api: SeoService) {}

  ngOnInit(): void { this.api.overview().subscribe((d) => this.data.set(d)); }

  link(it: SeoIssueItem): any[] { return it.type === 'category' ? ['/categories', it.id] : ['/products', it.id]; }
  sev(s: string): 'warn' | 'danger' | 'info' { return s === 'danger' ? 'danger' : s === 'info' ? 'info' : 'warn'; }
}
