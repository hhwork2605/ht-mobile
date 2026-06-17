import { OrderStatus } from './order.models';

export interface RecentOrder {
  id: number;
  code: string;
  recipient: string;
  total: number;
  status: OrderStatus;
  createdAt: string;
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
}
