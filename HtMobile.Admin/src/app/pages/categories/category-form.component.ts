import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { TextareaModule } from 'primeng/textarea';
import { SelectModule } from 'primeng/select';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { CategoriesService } from '../../core/categories.service';

@Component({
  selector: 'app-category-form',
  standalone: true,
  imports: [FormsModule, RouterLink, InputTextModule, InputNumberModule, TextareaModule, SelectModule, ButtonModule, ToastModule],
  providers: [MessageService],
  template: `
    <p-toast />
    <a routerLink="/categories" class="back"><i class="pi pi-arrow-left"></i> Tất cả danh mục</a>
    <h1 class="h1">{{ isNew ? 'Thêm danh mục' : 'Sửa danh mục' }}</h1>

    <section class="card">
      <div class="row2">
        <div class="f">
          <label>Tên danh mục *</label>
          <input pInputText [(ngModel)]="name" class="w-full" placeholder="VD: iPhone" />
        </div>
        <div class="f">
          <label>Slug (bỏ trống = tự sinh)</label>
          <input pInputText [(ngModel)]="slug" class="w-full" placeholder="iphone" />
        </div>
        <div class="f">
          <label>Danh mục cha</label>
          <p-select [options]="parentOptions" [(ngModel)]="parentId" optionLabel="label" optionValue="value"
                    placeholder="— Không (gốc) —" styleClass="w-full" appendTo="body" />
        </div>
        <div class="f">
          <label>Thứ tự</label>
          <p-inputnumber [(ngModel)]="sortOrder" [min]="0" styleClass="w-full" inputStyleClass="w-full" />
        </div>
        <div class="f span2">
          <label>Nội dung SEO (tuỳ chọn)</label>
          <textarea pTextarea [(ngModel)]="seoContent" rows="3" class="w-full"></textarea>
        </div>
      </div>
      <div class="actions">
        <p-button [label]="isNew ? 'Tạo' : 'Lưu'" icon="pi pi-check" [loading]="saving()" (onClick)="save()" />
        <p-button label="Quay lại" severity="secondary" [outlined]="true" routerLink="/categories" />
      </div>
    </section>
  `,
  styles: [`
    .back { display: inline-flex; align-items: center; gap: 6px; color: var(--ht-ink5); text-decoration: none; font-size: 13px; margin-bottom: 8px; }
    .h1 { font-size: 20px; font-weight: 700; margin: 0 0 16px; }
    .card { background: #fff; border: 1px solid var(--ht-line); border-radius: 14px; padding: 20px; max-width: 640px; }
    .row2 { display: grid; grid-template-columns: 1fr 1fr; gap: 14px; }
    .f { display: flex; flex-direction: column; } .span2 { grid-column: 1 / -1; }
    .f label { font-size: 12.5px; font-weight: 600; color: var(--ht-ink5); margin-bottom: 6px; }
    .w-full { width: 100%; }
    .actions { display: flex; gap: 10px; margin-top: 18px; }
  `],
})
export class CategoryFormComponent implements OnInit {
  isNew = true;
  id = 0;
  saving = signal(false);
  parentOptions: { label: string; value: number | null }[] = [{ label: '— Không (gốc) —', value: null }];

  name = ''; slug = ''; parentId: number | null = null; sortOrder = 0; seoContent = '';

  constructor(private api: CategoriesService, private route: ActivatedRoute, private router: Router, private toast: MessageService) {}

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    this.isNew = idParam === 'new' || !idParam;
    this.id = this.isNew ? 0 : Number(idParam);
    this.api.options().subscribe((opts) => {
      // Không cho chọn chính nó làm cha.
      this.parentOptions = [{ label: '— Không (gốc) —', value: null },
        ...opts.filter((o) => o.id !== this.id).map((o) => ({ label: o.name, value: o.id as number | null }))];
    });
    if (!this.isNew) this.load();
  }

  load(): void {
    this.api.get(this.id).subscribe((c) => {
      this.name = c.name; this.slug = c.slug; this.parentId = c.parentId ?? null;
      this.sortOrder = c.sortOrder; this.seoContent = c.seoContent ?? '';
    });
  }

  save(): void {
    if (!this.name.trim()) { this.toast.add({ severity: 'warn', summary: 'Nhập tên danh mục' }); return; }
    this.saving.set(true);
    const input = { name: this.name, slug: this.slug.trim() || null, parentId: this.parentId, sortOrder: this.sortOrder, seoContent: this.seoContent.trim() || null };
    const done = () => { this.toast.add({ severity: 'success', summary: 'Đã lưu' }); this.router.navigate(['/categories']); };
    const fail = (e: any) => { this.saving.set(false); this.toast.add({ severity: 'error', summary: 'Lỗi', detail: e?.error?.message ?? 'Lưu thất bại.' }); };
    if (this.isNew) this.api.create(input).subscribe({ next: done, error: fail });
    else this.api.update(this.id, input).subscribe({ next: done, error: fail });
  }
}
