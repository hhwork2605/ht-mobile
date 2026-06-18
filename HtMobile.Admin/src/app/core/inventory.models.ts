export interface StoreRow { id: number; name: string; address?: string | null; phone?: string | null; }
export interface StoreInput { name: string; address?: string | null; phone?: string | null; }

export interface StockRow {
  productId: number;
  productName: string;
  variantLabel: string;
  sku?: string | null;
  quantity: number;
}
export interface StockUpdateItem { productId: number; quantity: number; }
