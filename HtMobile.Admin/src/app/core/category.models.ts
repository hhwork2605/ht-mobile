export interface CategoryOption { id: number; name: string; }

export interface AdminCategoryRow {
  id: number;
  name: string;
  slug: string;
  parentName?: string | null;
  sortOrder: number;
  productCount: number;
  childCount: number;
}

export interface AdminCategoryDetail {
  id: number;
  name: string;
  slug: string;
  parentId?: number | null;
  sortOrder: number;
  seoContent?: string | null;
}

export interface CategoryInput {
  name: string;
  slug?: string | null;
  parentId?: number | null;
  sortOrder: number;
  seoContent?: string | null;
}
