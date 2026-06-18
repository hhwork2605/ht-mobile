export interface DayPoint { date: string; revenue: number; orders: number; }

export interface TopProductRow {
  productId: number;
  name: string;
  variantLabel: string;
  quantitySold: number;
  revenue: number;
}

export interface SalesReport {
  from: string;
  to: string;
  revenueTotal: number;
  ordersCount: number;
  avgOrderValue: number;
  cancelRate: number;
  byDay: DayPoint[];
  topProducts: TopProductRow[];
}
