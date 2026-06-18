export enum OrderStatus {
  Pending = 0, Confirmed = 1, Processing = 2, Shipped = 3, Delivered = 4, Cancelled = 5, Refunded = 6,
}

export interface AdminOrderRow {
  id: number;
  code: string;
  recipient: string;
  itemsSummary: string;
  total: number;
  paymentMethod?: string | null;
  status: OrderStatus;
  createdAt: string;
}

export interface AdminOrderListDto {
  orders: AdminOrderRow[];
  counts: Record<string, number>;   // key = OrderStatus dạng "0".."6"
  totalCount: number;
  filter?: OrderStatus | null;
}

export interface AdminOrderLine {
  productName: string;
  variantText: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

export interface AdminOrderDetailDto {
  id: number;
  code: string;
  status: OrderStatus;
  createdAt: string;
  subtotal: number;
  discountAmount: number;
  couponCode?: string | null;
  total: number;
  paymentMethod?: string | null;
  shipmentAddress?: string | null;
  customerId?: number | null;
  customerName?: string | null;
  customerEmail?: string | null;
  customerPhone?: string | null;
  isRegistered: boolean;
  items: AdminOrderLine[];
  allowedNext: OrderStatus[];
}

type Severity = 'success' | 'info' | 'warn' | 'danger' | 'secondary';

const LABELS: Record<OrderStatus, string> = {
  [OrderStatus.Pending]: 'Chờ xác nhận',
  [OrderStatus.Confirmed]: 'Đã xác nhận',
  [OrderStatus.Processing]: 'Đang xử lý',
  [OrderStatus.Shipped]: 'Đang giao',
  [OrderStatus.Delivered]: 'Hoàn thành',
  [OrderStatus.Cancelled]: 'Đã huỷ',
  [OrderStatus.Refunded]: 'Hoàn tiền',
};

const SEVERITY: Record<OrderStatus, Severity> = {
  [OrderStatus.Pending]: 'warn',
  [OrderStatus.Confirmed]: 'info',
  [OrderStatus.Processing]: 'info',
  [OrderStatus.Shipped]: 'info',
  [OrderStatus.Delivered]: 'success',
  [OrderStatus.Cancelled]: 'danger',
  [OrderStatus.Refunded]: 'secondary',
};

export const ORDER_STATUSES: OrderStatus[] = [
  OrderStatus.Pending, OrderStatus.Confirmed, OrderStatus.Processing,
  OrderStatus.Shipped, OrderStatus.Delivered, OrderStatus.Cancelled, OrderStatus.Refunded,
];

export function orderStatusLabel(s: OrderStatus): string { return LABELS[s] ?? String(s); }
export function orderStatusSeverity(s: OrderStatus): Severity { return SEVERITY[s] ?? 'secondary'; }
