export enum ProductStatus { Active = 0, OutOfStock = 1, Discontinued = 2 }

export interface AdminCategoryOption { id: number; name: string; }

export interface AdminProductRow {
  id: number;
  name: string;
  slug: string;
  categoryName: string;
  sku?: string | null;
  variantCount: number;
  priceFrom: number;
  priceTo: number;
  isActive: boolean;
}

export interface AdminProductListDto {
  products: AdminProductRow[];
  categories: AdminCategoryOption[];
  categoryId?: number | null;
}

export interface AdminVariantRow {
  id: number;
  sku: string;
  storage?: string | null;
  color?: string | null;
  basePrice: number;
  compareAtPrice?: number | null;
  status: ProductStatus;
}

export interface AdminProductEditDto {
  id: number;
  name: string;
  slug: string;
  categoryId: number;
  brand?: string | null;
  tagline?: string | null;
  description?: string | null;
  specs?: string | null;
  variants: AdminVariantRow[];
  categories: AdminCategoryOption[];
}

export interface ProductInput {
  name: string;
  slug?: string | null;
  categoryId: number;
  brand?: string | null;
  tagline?: string | null;
  description?: string | null;
  specs?: string | null;
}

export interface VariantInput {
  sku: string;
  storage?: string | null;
  color?: string | null;
  basePrice: number;
  compareAtPrice?: number | null;
  status: ProductStatus;
}

export interface VariantEdit {
  id: number;
  basePrice: number;
  compareAtPrice?: number | null;
  status: ProductStatus;
}

export interface ProductImageRow { id: number; url: string; sortOrder: number; }

export interface AuthUser { email: string; fullName: string; roles: string[]; }
