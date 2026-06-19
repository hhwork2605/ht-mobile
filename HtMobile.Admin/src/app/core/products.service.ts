import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  AdminProductListDto, AdminProductEditDto, AdminCategoryOption,
  ProductInput, VariantInput, VariantEdit, ProductImageRow,
} from './models';

@Injectable({ providedIn: 'root' })
export class ProductsService {
  private base = `${environment.apiBase}/products`;

  constructor(private http: HttpClient) {}

  list(categoryId?: number | null): Observable<AdminProductListDto> {
    let params = new HttpParams();
    if (categoryId) params = params.set('category', categoryId);
    return this.http.get<AdminProductListDto>(this.base, { params });
  }

  get(id: number): Observable<AdminProductEditDto> {
    return this.http.get<AdminProductEditDto>(`${this.base}/${id}`);
  }

  categories(): Observable<AdminCategoryOption[]> {
    return this.http.get<AdminCategoryOption[]>(`${environment.apiBase}/categories`);
  }

  create(product: ProductInput, variant: VariantInput): Observable<{ id: number }> {
    return this.http.post<{ id: number }>(this.base, { product, variant });
  }

  update(id: number, product: ProductInput, variants: VariantEdit[]): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}`, { product, variants });
  }

  addVariant(id: number, input: VariantInput): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/variants`, input);
  }

  toggleVariant(variantId: number): Observable<{ productId: number }> {
    return this.http.post<{ productId: number }>(`${this.base}/variants/${variantId}/toggle`, {});
  }

  // ===== Ảnh sản phẩm =====
  images(id: number): Observable<ProductImageRow[]> {
    return this.http.get<ProductImageRow[]>(`${this.base}/${id}/images`);
  }

  uploadImage(id: number, file: File): Observable<ProductImageRow> {
    const fd = new FormData();
    fd.append('file', file);
    return this.http.post<ProductImageRow>(`${this.base}/${id}/images`, fd);
  }

  deleteImage(imageId: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/images/${imageId}`);
  }

  reorderImages(id: number, orderedIds: number[]): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}/images/order`, orderedIds);
  }
}
