import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { TextareaModule } from 'primeng/textarea';
import { SelectModule } from 'primeng/select';
import { DatePickerModule } from 'primeng/datepicker';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { PromotionsService } from '../../core/promotions.service';
import { PROMOTION_TYPE_OPTIONS, PromotionType } from '../../core/promotion.models';

@Component({
  selector: 'app-promotion-form',
  standalone: true,
  imports: [FormsModule, RouterLink, InputTextModule, InputNumberModule, TextareaModule,
    SelectModule, DatePickerModule, ButtonModule, ToastModule],
  providers: [MessageService],
  template: `
    <p-toast />
    <a routerLink="/promotions" class="back"><i class="pi pi-arrow-left"></i> Tất cả khuyến mãi</a>
    <h1 class="h1">{{ isNew ? 'Thêm khuyến mãi' : 'Sửa khuyến mãi' }}</h1>

    <section class="card">
      <div class="row2">
        <div class="f span2">
          <label>Tên chương trình *</label>
          <input pInputText [(ngModel)]="name" class="w-full" placeholder="VD: Ưu đãi khai trương -10%" />
        </div>
        <div class="f span2">
          <label>Mã giảm giá (voucher) — để trống = KM tự động</label>
          <input pInputText [(ngModel)]="code" class="w-full mono" placeholder="VD: GIAM5" />
          <small class="hint">Có mã → khách phải NHẬP mã ở giỏ/checkout mới được giảm. Để trống → tự áp theo điều kiện. Ngưỡng đơn tối thiểu: thêm <code>{{ '{' }}"minOrder":10000000{{ '}' }}</code> vào Điều kiện.</small>
        </div>
        <div class="f">
          <label>Loại *</label>
          <p-select [options]="typeOptions" [(ngModel)]="type" optionLabel="label" optionValue="value" styleClass="w-full" appendTo="body" />
        </div>
        <div class="f">
          <label>Giá trị {{ valueHint }}</label>
          <p-inputnumber [(ngModel)]="value" [min]="0" styleClass="w-full" inputStyleClass="w-full" />
        </div>
        <div class="f">
          <label>Bắt đầu *</label>
          <p-datepicker [(ngModel)]="startsAt" [showTime]="true" [showIcon]="true" dateFormat="dd/mm/yy" styleClass="w-full" appendTo="body" />
        </div>
        <div class="f">
          <label>Kết thúc *</label>
          <p-datepicker [(ngModel)]="endsAt" [showTime]="true" [showIcon]="true" dateFormat="dd/mm/yy" styleClass="w-full" appendTo="body" />
        </div>
        <div class="f span2">
          <label>Điều kiện (JSON, tuỳ chọn)</label>
          <textarea pTextarea [(ngModel)]="conditionsJson" rows="3" class="w-full mono"
                    placeholder='{"categoryIds":[1],"productIds":[],"variantIds":[]}'></textarea>
          <small class="hint">Bỏ trống = áp cho mọi sản phẩm. variantIds = Id biến thể con, productIds = Id model cha.</small>
        </div>
      </div>

      <div class="actions">
        <p-button [label]="isNew ? 'Tạo' : 'Lưu'" icon="pi pi-check" [loading]="saving()" (onClick)="save()" />
        <p-button label="Quay lại" severity="secondary" [outlined]="true" routerLink="/promotions" />
      </div>
    </section>
  `,
  styles: [`
    .back { display: inline-flex; align-items: center; gap: 6px; color: var(--ht-ink5); text-decoration: none; font-size: 13px; margin-bottom: 8px; }
    .h1 { font-size: 20px; font-weight: 700; margin: 0 0 16px; }
    .card { background: #fff; border: 1px solid var(--ht-line); border-radius: 14px; padding: 20px; max-width: 720px; }
    .row2 { display: grid; grid-template-columns: 1fr 1fr; gap: 14px; }
    .f { display: flex; flex-direction: column; } .span2 { grid-column: 1 / -1; }
    .f label { font-size: 12.5px; font-weight: 600; color: var(--ht-ink5); margin-bottom: 6px; }
    .w-full { width: 100%; } .mono { font-family: ui-monospace, monospace; font-size: 12.5px; }
    .hint { color: var(--ht-ink4); font-size: 11.5px; margin-top: 4px; }
    .actions { display: flex; gap: 10px; margin-top: 18px; }
  `],
})
export class PromotionFormComponent implements OnInit {
  isNew = true;
  id = 0;
  saving = signal(false);
  typeOptions = PROMOTION_TYPE_OPTIONS;

  name = '';
  code = '';
  type: PromotionType = PromotionType.Percentage;
  value = 0;
  startsAt: Date = new Date();
  endsAt: Date = new Date(Date.now() + 7 * 86400000);
  conditionsJson = '';

  constructor(private api: PromotionsService, private route: ActivatedRoute, private router: Router, private toast: MessageService) {}

  get valueHint(): string {
    return this.type === PromotionType.Percentage ? '(%)' : this.type === PromotionType.FixedAmount ? '(₫)' : '';
  }

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    this.isNew = idParam === 'new' || !idParam;
    if (!this.isNew) { this.id = Number(idParam); this.load(); }
  }

  load(): void {
    this.api.get(this.id).subscribe((p) => {
      this.name = p.name; this.code = p.code ?? ''; this.type = p.type; this.value = p.value;
      this.startsAt = new Date(p.startsAt); this.endsAt = new Date(p.endsAt);
      this.conditionsJson = p.conditionsJson ?? '';
    });
  }

  save(): void {
    if (!this.name.trim()) { this.toast.add({ severity: 'warn', summary: 'Nhập tên chương trình' }); return; }
    if (this.endsAt < this.startsAt) { this.toast.add({ severity: 'warn', summary: 'Ngày kết thúc phải sau ngày bắt đầu' }); return; }
    this.saving.set(true);
    const input = {
      name: this.name, code: this.code.trim() || null, type: this.type, value: this.value,
      startsAt: this.startsAt.toISOString(), endsAt: this.endsAt.toISOString(),
      conditionsJson: this.conditionsJson.trim() || null,
    };
    const done = () => { this.toast.add({ severity: 'success', summary: 'Đã lưu' }); this.router.navigate(['/promotions']); };
    const fail = (e: any) => { this.saving.set(false); this.toast.add({ severity: 'error', summary: 'Lỗi', detail: e?.error?.message ?? 'Lưu thất bại.' }); };

    if (this.isNew) this.api.create(input).subscribe({ next: done, error: fail });
    else this.api.update(this.id, input).subscribe({ next: done, error: fail });
  }
}
