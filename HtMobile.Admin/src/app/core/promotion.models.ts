export enum PromotionType {
  Percentage = 0, FixedAmount = 1, Gift = 2, Voucher = 3, Combo = 4, BankOffer = 5,
}

export interface AdminPromotionRow {
  id: number;
  name: string;
  code?: string | null;
  type: PromotionType;
  value: number;
  startsAt: string;
  endsAt: string;
  isActiveNow: boolean;
}

export interface AdminPromotionDetail {
  id: number;
  name: string;
  code?: string | null;
  type: PromotionType;
  value: number;
  startsAt: string;
  endsAt: string;
  conditionsJson?: string | null;
}

export interface PromotionInput {
  name: string;
  code?: string | null;
  type: PromotionType;
  value: number;
  startsAt: string;
  endsAt: string;
  conditionsJson?: string | null;
}

const TYPE_LABELS: Record<PromotionType, string> = {
  [PromotionType.Percentage]: 'Giảm %',
  [PromotionType.FixedAmount]: 'Giảm tiền',
  [PromotionType.Gift]: 'Quà tặng',
  [PromotionType.Voucher]: 'Voucher',
  [PromotionType.Combo]: 'Combo',
  [PromotionType.BankOffer]: 'Ưu đãi ngân hàng',
};

export const PROMOTION_TYPE_OPTIONS = Object.entries(TYPE_LABELS)
  .map(([value, label]) => ({ value: Number(value) as PromotionType, label }));

export function promotionTypeLabel(t: PromotionType): string { return TYPE_LABELS[t] ?? String(t); }

/** Hiển thị giá trị KM theo loại: % cho Percentage, tiền cho FixedAmount, còn lại "—". */
export function promotionValueText(r: { type: PromotionType; value: number }): string {
  if (r.type === PromotionType.Percentage) return `${r.value}%`;
  if (r.type === PromotionType.FixedAmount) return `${r.value.toLocaleString('vi-VN')}₫`;
  return '—';
}
