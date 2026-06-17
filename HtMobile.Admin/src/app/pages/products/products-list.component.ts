import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { SelectModule } from 'primeng/select';
import { ProductsService } from '../../core/products.service';
import { AdminProductRow, AdminCategoryOption } from '../../core/models';

@Component({
  selector: 'app-products-list',
  standalone: true,
  imports: [FormsModule, RouterLink, TableModule, ButtonModule, TagModule, SelectModule],
  template: `
    <div class="head">
      <h1 class="h1">Sản phẩm</h1>
      <p-button label="Thêm sản phẩm" icon="pi pi-plus" routerLink="/products/new" />
    </div>

    <div class="card">
      <div class="toolbar">
        <p-select [options]="categoryOptions" [(ngModel)]="categoryId" optionLabel="name" optionValue="id"
                  placeholder="Tất cả danh mục" [showClear]="true" (onChange)="reload()" styleClass="w-64" />
        <span class="count">{{ rows().length }} sản phẩm</span>
      </div>

      <p-table [value]="rows()" [loading]="loading()" [paginator]="rows().length > 12" [rows]="12"
               styleClass="p-datatable-sm" [tableStyle]="{ 'min-width': '60rem' }">
        <ng-template pTemplate="header">
          <tr>
            <th>Sản phẩm</th>
            <th>Danh mục</th>
            <th class="ta-c">Biến thể</th>
            <th class="ta-r">Giá bán</th>
            <th class="ta-c">Trạng thái</th>
            <th></th>
          </tr>
        </ng-template>
        <ng-template pTemplate="body" let-p>
          <tr>
            <td>
              <div class="pname">{{ p.name }}</div>
              <div class="psku">{{ p.sku }}</div>
            </td>
            <td class="muted">{{ p.categoryName }}</td>
            <td class="ta-c muted">{{ p.variantCount }}</td>
            <td class="ta-r strong">{{ priceText(p) }}</td>
            <td class="ta-c">
              <p-tag [value]="p.isActive ? 'Đang bán' : 'Ẩn'" [severity]="p.isActive ? 'success' : 'danger'" />
            </td>
            <td class="ta-r">
              <p-button label="Sửa" [text]="true" size="small" [routerLink]="['/products', p.id]" />
            </td>
          </tr>
        </ng-template>
        <ng-template pTemplate="emptymessage">
          <tr><td colspan="6" class="empty">Chưa có sản phẩm nào.</td></tr>
        </ng-template>
      </p-table>
    </div>
  `,
  styles: [`
    .head { display: flex; align-items: center; justify-content: space-between; margin-bottom: 16px; }
    .h1 { font-size: 20px; font-weight: 700; margin: 0; }
    .card { background: #fff; border: 1px solid var(--ht-line); border-radius: 14px; overflow: hidden; }
    .toolbar { display: flex; align-items: center; gap: 12px; padding: 14px 16px; border-bottom: 1px solid var(--ht-line); }
    .count { margin-left: auto; font-size: 12.5px; color: var(--ht-ink4); }
    .w-64 { width: 16rem; }
    .pname { font-weight: 600; }
    .psku { font-size: 11.5px; color: var(--ht-ink4); }
    .muted { color: var(--ht-ink5); }
    .strong { font-weight: 700; }
    .ta-c { text-align: center; } .ta-r { text-align: right; }
    .empty { text-align: center; padding: 28px; color: var(--ht-ink4); }
  `],
})
export class ProductsListComponent implements OnInit {
  rows = signal<AdminProductRow[]>([]);
  loading = signal(false);
  categoryOptions: AdminCategoryOption[] = [];
  categoryId: number | null = null;

  constructor(private api: ProductsService) {}

  ngOnInit(): void { this.reload(); }

  reload(): void {
    this.loading.set(true);
    this.api.list(this.categoryId).subscribe({
      next: (dto) => {
        this.rows.set(dto.products);
        this.categoryOptions = dto.categories;
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  priceText(p: AdminProductRow): string {
    const f = this.vnd(p.priceFrom);
    return p.priceFrom === p.priceTo ? f : `${this.vnd(p.priceFrom)} – ${this.vnd(p.priceTo)}`;
  }

  private vnd(n: number): string { return n.toLocaleString('vi-VN') + '₫'; }
}
