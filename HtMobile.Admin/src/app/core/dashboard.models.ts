import { OrderStatus } from './order.models';

export interface RecentOrder {
  id: number;
  code: string;
  recipient: string;
  total: number;
  status: OrderStatus;
  createdAt: string;
}

export interface DashboardDayPoint { date: string; revenue: number; }

export interface DashboardTopProduct {
  productId: number;
  name: string;
  variantLabel: string;
  quantitySold: number;
  revenue: number;
}

export interface LowStockRow {
  productId: number;
  name: string;
  variantLabel: string;
  totalQuantity: number;
}

export interface DashboardStats {
  revenueTotal: number;
  revenueToday: number;
  ordersTotal: number;
  ordersToday: number;
  productsCount: number;
  customersCount: number;
  statusCounts: Record<string, number>;
  recentOrders: RecentOrder[];
  revenueByDay: DashboardDayPoint[];
  topProducts: DashboardTopProduct[];
  lowStock: LowStockRow[];
  lowStockCount: number;
  lowStockThreshold: number;
}
