import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { TextareaModule } from 'primeng/textarea';
import { SelectModule } from 'primeng/select';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { ProductsService } from '../../core/products.service';
import {
  AdminCategoryOption, AdminVariantRow, ProductInput, ProductStatus, VariantInput,
} from '../../core/models';

@Component({
  selector: 'app-product-form',
  standalone: true,
  imports: [FormsModule, RouterLink, InputTextModule, InputNumberModule, TextareaModule,
    SelectModule, ButtonModule, TableModule, TagModule, ToastModule],
  providers: [MessageService],
  template: `
    <p-toast />
    <a routerLink="/products" class="back"><i class="pi pi-arrow-left"></i> Tất cả sản phẩm</a>
    <h1 class="h1">{{ isNew ? 'Thêm sản phẩm' : 'Sửa sản phẩm' }}</h1>

    <div class="grid">
      <!-- Thông tin sản phẩm -->
      <section class="card">
        <h2 class="h2">Thông tin sản phẩm</h2>
        <div class="row2">
          <div class="f span2">
            <label>Tên sản phẩm *</label>
            <input pInputText [(ngModel)]="product.name" class="w-full" placeholder="VD: iPhone 17 Pro Max" />
          </div>
          <div class="f">
            <label>Slug (bỏ trống = tự sinh)</label>
            <input pInputText [(ngModel)]="product.slug" class="w-full" placeholder="iphone-17-pro-max" />
          </div>
          <div class="f">
            <label>Danh mục *</label>
            <p-select [options]="categories" [(ngModel)]="product.categoryId" optionLabel="name" optionValue="id"
                      placeholder="— Chọn —" styleClass="w-full" appendTo="body" />
          </div>
          <div class="f"><label>Thương hiệu</label><input pInputText [(ngModel)]="product.brand" class="w-full" /></div>
          <div class="f"><label>Tagline</label><input pInputText [(ngModel)]="product.tagline" class="w-full" /></div>
          <div class="f span2"><label>Mô tả</label><textarea pTextarea [(ngModel)]="product.description" rows="3" class="w-full"></textarea></div>
          <div class="f span2">
            <label>Thông số kỹ thuật (JSON)</label>
            <textarea pTextarea [(ngModel)]="product.specs" rows="6" class="w-full mono"
                      placeholder='[{{ "{" }}"label":"Màn hình","value":"6.9&quot;"{{ "}" }}]'></textarea>
            <small class="hint2">Mảng <code>[{{ "{" }}"label","value"{{ "}" }}]</code> hoặc nhóm <code>[{{ "{" }}"group","items":[…]{{ "}" }}]</code>. Để trống nếu chưa có.</small>
          </div>
        </div>
      </section>

      @if (isNew) {
        <!-- Biến thể đầu tiên (tạo mới) -->
        <section class="card">
          <h2 class="h2">Biến thể đầu tiên</h2>
          <div class="row2">
            <div class="f"><label>SKU *</label><input pInputText [(ngModel)]="variant.sku" class="w-full" /></div>
            <div class="f"><label>Dung lượng</label><input pInputText [(ngModel)]="variant.storage" class="w-full" placeholder="256GB" /></div>
            <div class="f"><label>Màu</label><input pInputText [(ngModel)]="variant.color" class="w-full" placeholder="Đen" /></div>
            <div class="f"><label>Trạng thái</label><p-select [options]="statusOptions" [(ngModel)]="variant.status" optionLabel="label" optionValue="value" styleClass="w-full" appendTo="body" /></div>
            <div class="f"><label>Giá bán (₫) *</label><p-inputnumber [(ngModel)]="variant.basePrice" [min]="0" styleClass="w-full" inputStyleClass="w-full" /></div>
            <div class="f"><label>Giá gạch (₫)</label><p-inputnumber [(ngModel)]="variant.compareAtPrice" [min]="0" styleClass="w-full" inputStyleClass="w-full" /></div>
          </div>
        </section>
      } @else {
        <!-- Biến thể (sửa) -->
        <section class="card">
          <h2 class="h2">Biến thể</h2>
          <p class="hint">Sửa giá rồi bấm <b>Lưu thay đổi</b>. Nút Ẩn/Hiện lưu ngay.</p>
          <p-table [value]="variants()" styleClass="p-datatable-sm">
            <ng-template pTemplate="header">
              <tr><th>Biến thể</th><th>Giá bán (₫)</th><th>Giá gạch (₫)</th><th>Trạng thái</th><th></th></tr>
            </ng-template>
            <ng-template pTemplate="body" let-v>
              <tr>
                <td><div class="strong">{{ v.sku }}</div><div class="muted">{{ label(v) }}</div></td>
                <td><p-inputnumber [(ngModel)]="v.basePrice" [min]="0" inputStyleClass="w-32" /></td>
                <td><p-inputnumber [(ngModel)]="v.compareAtPrice" [min]="0" inputStyleClass="w-32" /></td>
                <td><p-tag [value]="v.status === 0 ? 'Đang bán' : 'Ẩn'" [severity]="v.status === 0 ? 'success' : 'danger'" /></td>
                <td class="ta-r"><p-button [label]="v.status === 0 ? 'Ẩn' : 'Hiện'" severity="secondary" [outlined]="true" size="small" (onClick)="toggle(v)" /></td>
              </tr>
            </ng-template>
          </p-table>

          <!-- Thêm biến thể -->
          <div class="addbar">
            <input pInputText [(ngModel)]="newVar.sku" placeholder="SKU *" class="sm" />
            <input pInputText [(ngModel)]="newVar.storage" placeholder="Dung lượng" class="sm" />
            <input pInputText [(ngModel)]="newVar.color" placeholder="Màu" class="sm" />
            <p-inputnumber [(ngModel)]="newVar.basePrice" [min]="0" placeholder="Giá *" inputStyleClass="w-28" />
            <p-button label="Thêm biến thể" icon="pi pi-plus" severity="secondary" [outlined]="true" size="small" (onClick)="addVariant()" />
          </div>
        </section>
      }

      <div class="actions">
        <p-button [label]="isNew ? 'Tạo sản phẩm' : 'Lưu thay đổi'" icon="pi pi-check" [loading]="saving()" (onClick)="save()" />
        <p-button label="Quay lại" severity="secondary" [outlined]="true" routerLink="/products" />
      </div>
    </div>
  `,
  styles: [`
    .back { display: inline-flex; align-items: center; gap: 6px; color: var(--ht-ink5); text-decoration: none; font-size: 13px; margin-bottom: 8px; }
    .h1 { font-size: 20px; font-weight: 700; margin: 0 0 16px; }
    .grid { max-width: 760px; display: flex; flex-direction: column; gap: 16px; }
    .card { background: #fff; border: 1px solid var(--ht-line); border-radius: 14px; padding: 20px; }
    .h2 { font-size: 15px; font-weight: 700; margin: 0 0 14px; }
    .hint { font-size: 12.5px; color: var(--ht-ink5); margin: 0 0 12px; }
    .row2 { display: grid; grid-template-columns: 1fr 1fr; gap: 14px; }
    .f { display: flex; flex-direction: column; } .span2 { grid-column: 1 / -1; }
    .f label { font-size: 12.5px; font-weight: 600; color: var(--ht-ink5); margin-bottom: 6px; }
    .w-full { width: 100%; }
    .strong { font-weight: 600; } .muted { font-size: 11.5px; color: var(--ht-ink4); }
    .ta-r { text-align: right; }
    .addbar { display: flex; flex-wrap: wrap; gap: 8px; align-items: center; margin-top: 14px; padding-top: 14px; border-top: 1px solid var(--ht-line); }
    .addbar .sm { width: 140px; }
    .actions { display: flex; gap: 10px; }
    .mono { font-family: ui-monospace, "SF Mono", Menlo, monospace; font-size: 12.5px; }
    .hint2 { font-size: 11.5px; color: var(--ht-ink4); margin-top: 5px; }
    .hint2 code { background: var(--ht-line); border-radius: 4px; padding: 1px 5px; }
  `],
})
export class ProductFormComponent implements OnInit {
  isNew = true;
  id = 0;
  saving = signal(false);
  categories: AdminCategoryOption[] = [];
  variants = signal<AdminVariantRow[]>([]);

  product: ProductInput = { name: '', slug: '', categoryId: 0, brand: 'Apple', tagline: '', description: '', specs: '' };
  variant: VariantInput = { sku: '', storage: '', color: '', basePrice: 0, compareAtPrice: null, status: ProductStatus.Active };
  newVar: VariantInput = { sku: '', storage: '', color: '', basePrice: 0, compareAtPrice: null, status: ProductStatus.Active };

  statusOptions = [
    { label: 'Đang bán', value: ProductStatus.Active },
    { label: 'Hết hàng', value: ProductStatus.OutOfStock },
    { label: 'Ngừng KD', value: ProductStatus.Discontinued },
  ];

  constructor(
    private api: ProductsService, private route: ActivatedRoute,
    private router: Router, private toast: MessageService,
  ) {}

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    this.isNew = idParam === 'new' || !idParam;
    if (this.isNew) {
      this.api.categories().subscribe((c) => (this.categories = c));
    } else {
      this.id = Number(idParam);
      this.load();
    }
  }

  load(): void {
    this.api.get(this.id).subscribe((dto) => {
      this.product = { name: dto.name, slug: dto.slug, categoryId: dto.categoryId, brand: dto.brand, tagline: dto.tagline, description: dto.description, specs: dto.specs };
      this.categories = dto.categories;
      this.variants.set(dto.variants);
    });
  }

  label(v: AdminVariantRow): string { return [v.storage, v.color].filter(Boolean).join(' · '); }

  save(): void {
    this.saving.set(true);
    if (this.isNew) {
      this.api.create(this.product, this.variant).subscribe({
        next: (r) => { this.toast.add({ severity: 'success', summary: 'Đã tạo sản phẩm' }); this.router.navigate(['/products', r.id]); },
        error: (e) => { this.fail(e); },
      });
    } else {
      const edits = this.variants().map((v) => ({ id: v.id, basePrice: v.basePrice, compareAtPrice: v.compareAtPrice, status: v.status }));
      this.api.update(this.id, this.product, edits).subscribe({
        next: () => { this.saving.set(false); this.toast.add({ severity: 'success', summary: 'Đã lưu thay đổi' }); this.load(); },
        error: (e) => { this.fail(e); },
      });
    }
  }

  addVariant(): void {
    if (!this.newVar.sku) { this.toast.add({ severity: 'warn', summary: 'Nhập SKU biến thể' }); return; }
    this.api.addVariant(this.id, this.newVar).subscribe({
      next: () => { this.toast.add({ severity: 'success', summary: 'Đã thêm biến thể' }); this.newVar = { sku: '', storage: '', color: '', basePrice: 0, compareAtPrice: null, status: ProductStatus.Active }; this.load(); },
      error: (e) => this.fail(e, false),
    });
  }

  toggle(v: AdminVariantRow): void {
    this.api.toggleVariant(v.id).subscribe({ next: () => this.load(), error: (e) => this.fail(e, false) });
  }

  private fail(e: any, resetSaving = true): void {
    if (resetSaving) this.saving.set(false);
    this.toast.add({ severity: 'error', summary: 'Lỗi', detail: e?.error?.message ?? 'Thao tác thất bại.' });
  }
}
