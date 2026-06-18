import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { SelectModule } from 'primeng/select';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService, MessageService } from 'primeng/api';
import { InventoryService } from '../../core/inventory.service';
import { StoreRow, StockRow } from '../../core/inventory.models';

@Component({
  selector: 'app-inventory',
  standalone: true,
  imports: [FormsModule, TableModule, SelectModule, InputTextModule, InputNumberModule,
    ButtonModule, DialogModule, ToastModule, ConfirmDialogModule],
  providers: [ConfirmationService, MessageService],
  template: `
    <p-toast />
    <p-confirmdialog />
    <h1 class="h1">Tồn kho theo cửa hàng</h1>

    <div class="bar">
      <span class="lbl">Cửa hàng</span>
      <p-select [options]="stores()" [(ngModel)]="storeId" optionLabel="name" optionValue="id"
                placeholder="Chọn cửa hàng" styleClass="store-sel" appendTo="body" (onChange)="loadStock()" />
      <p-button label="Thêm" icon="pi pi-plus" size="small" (onClick)="openStore(null)" />
      @if (storeId) {
        <p-button label="Sửa" icon="pi pi-pencil" size="small" severity="secondary" [outlined]="true" (onClick)="openStore(current())" />
        <p-button label="Xoá" icon="pi pi-trash" size="small" severity="danger" [outlined]="true" (onClick)="deleteStore()" />
      }
    </div>

    @if (storeId) {
      <div class="card">
        <p-table [value]="stock()" [loading]="loading()" styleClass="p-datatable-sm" [tableStyle]="{ 'min-width': '46rem' }">
          <ng-template pTemplate="header">
            <tr><th>Biến thể</th><th>SKU</th><th class="ta-c" style="width:9rem">Tồn kho</th></tr>
          </ng-template>
          <ng-template pTemplate="body" let-r>
            <tr>
              <td><span class="strong">{{ r.productName }}</span> <span class="muted">{{ r.variantLabel }}</span></td>
              <td class="muted mono">{{ r.sku }}</td>
              <td class="ta-c"><p-inputnumber [(ngModel)]="r.quantity" [min]="0" [showButtons]="true" buttonLayout="horizontal"
                    inputStyleClass="qty-input" decrementButtonClass="p-button-secondary" incrementButtonClass="p-button-secondary"
                    incrementButtonIcon="pi pi-plus" decrementButtonIcon="pi pi-minus" /></td>
            </tr>
          </ng-template>
          <ng-template pTemplate="emptymessage"><tr><td colspan="3" class="empty">Chưa có biến thể nào.</td></tr></ng-template>
        </p-table>
        <div class="foot">
          <p-button label="Lưu tồn kho" icon="pi pi-check" [loading]="saving()" (onClick)="save()" />
        </div>
      </div>
    } @else if (stores().length === 0) {
      <p class="muted">Chưa có cửa hàng. Bấm <b>Thêm</b> để tạo cửa hàng đầu tiên.</p>
    }

    <p-dialog [header]="editing?.id ? 'Sửa cửa hàng' : 'Thêm cửa hàng'" [(visible)]="storeDialog" [modal]="true" [style]="{ width: '420px' }">
      <div class="form">
        <label>Tên cửa hàng *</label>
        <input pInputText [(ngModel)]="sName" class="w-full" />
        <label>Địa chỉ</label>
        <input pInputText [(ngModel)]="sAddress" class="w-full" />
        <label>Điện thoại</label>
        <input pInputText [(ngModel)]="sPhone" class="w-full" />
      </div>
      <ng-template pTemplate="footer">
        <p-button label="Huỷ" severity="secondary" [outlined]="true" (onClick)="storeDialog = false" />
        <p-button label="Lưu" icon="pi pi-check" [loading]="saving()" (onClick)="saveStore()" />
      </ng-template>
    </p-dialog>
  `,
  styles: [`
    .h1 { font-size: 20px; font-weight: 700; margin: 0 0 16px; }
    .bar { display: flex; align-items: center; gap: 10px; margin-bottom: 16px; flex-wrap: wrap; }
    .lbl { font-size: 13px; color: var(--ht-ink5); font-weight: 600; }
    :host ::ng-deep .store-sel { min-width: 260px; }
    .card { background: #fff; border: 1px solid var(--ht-line); border-radius: 14px; overflow: hidden; }
    .strong { font-weight: 600; } .muted { color: var(--ht-ink5); } .mono { font-family: ui-monospace, monospace; font-size: 12.5px; }
    .ta-c { text-align: center; }
    :host ::ng-deep .qty-input { width: 4.5rem; text-align: center; }
    .empty { text-align: center; padding: 24px; color: var(--ht-ink4); }
    .foot { display: flex; justify-content: flex-end; padding: 14px; border-top: 1px solid var(--ht-line); }
    .form { display: flex; flex-direction: column; gap: 6px; }
    .form label { font-size: 12.5px; font-weight: 600; color: var(--ht-ink5); margin-top: 8px; }
    .w-full { width: 100%; }
  `],
})
export class InventoryComponent implements OnInit {
  stores = signal<StoreRow[]>([]);
  stock = signal<StockRow[]>([]);
  loading = signal(false);
  saving = signal(false);
  storeId: number | null = null;

  storeDialog = false;
  editing: StoreRow | null = null;
  sName = ''; sAddress = ''; sPhone = '';

  constructor(private api: InventoryService, private confirm: ConfirmationService, private toast: MessageService) {}

  ngOnInit(): void { this.loadStores(); }

  current(): StoreRow | null { return this.stores().find((s) => s.id === this.storeId) ?? null; }

  loadStores(select?: number): void {
    this.api.stores().subscribe((s) => {
      this.stores.set(s);
      if (select) this.storeId = select;
      else if (this.storeId && !s.some((x) => x.id === this.storeId)) this.storeId = null;
      else if (!this.storeId && s.length) this.storeId = s[0].id;
      if (this.storeId) this.loadStock();
    });
  }

  loadStock(): void {
    if (!this.storeId) { this.stock.set([]); return; }
    this.loading.set(true);
    this.api.stock(this.storeId).subscribe({ next: (r) => { this.stock.set(r); this.loading.set(false); }, error: () => this.loading.set(false) });
  }

  save(): void {
    if (!this.storeId) return;
    this.saving.set(true);
    const items = this.stock().map((r) => ({ productId: r.productId, quantity: r.quantity }));
    this.api.setStock(this.storeId, items).subscribe({
      next: () => { this.saving.set(false); this.toast.add({ severity: 'success', summary: 'Đã lưu tồn kho' }); },
      error: () => { this.saving.set(false); this.toast.add({ severity: 'error', summary: 'Lỗi', detail: 'Lưu thất bại.' }); },
    });
  }

  openStore(s: StoreRow | null): void {
    this.editing = s;
    this.sName = s?.name ?? ''; this.sAddress = s?.address ?? ''; this.sPhone = s?.phone ?? '';
    this.storeDialog = true;
  }

  saveStore(): void {
    if (!this.sName.trim()) { this.toast.add({ severity: 'warn', summary: 'Nhập tên cửa hàng' }); return; }
    this.saving.set(true);
    const input = { name: this.sName, address: this.sAddress.trim() || null, phone: this.sPhone.trim() || null };
    const ok = (id?: number) => { this.saving.set(false); this.storeDialog = false; this.toast.add({ severity: 'success', summary: 'Đã lưu cửa hàng' }); this.loadStores(id); };
    const fail = (e: any) => { this.saving.set(false); this.toast.add({ severity: 'error', summary: 'Lỗi', detail: e?.error?.message ?? '' }); };
    if (this.editing?.id) this.api.updateStore(this.editing.id, input).subscribe({ next: () => ok(this.editing!.id), error: fail });
    else this.api.createStore(input).subscribe({ next: (r) => ok(r.id), error: fail });
  }

  deleteStore(): void {
    const s = this.current();
    if (!s) return;
    this.confirm.confirm({
      header: 'Xoá cửa hàng', message: `Xoá "${s.name}"?`, icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Xoá', rejectLabel: 'Huỷ', acceptButtonStyleClass: 'p-button-danger',
      accept: () => this.api.deleteStore(s.id).subscribe({
        next: () => { this.storeId = null; this.toast.add({ severity: 'success', summary: 'Đã xoá' }); this.loadStores(); },
        error: (e) => this.toast.add({ severity: 'error', summary: 'Không xoá được', detail: e?.error?.message ?? '' }),
      }),
    });
  }
}
