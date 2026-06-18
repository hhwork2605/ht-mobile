import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { AdminCategoryRow, AdminCategoryDetail, CategoryInput, CategoryOption } from './category.models';

@Injectable({ providedIn: 'root' })
export class CategoriesService {
  private base = `${environment.apiBase}/categories`;

  constructor(private http: HttpClient) {}

  options(): Observable<CategoryOption[]> { return this.http.get<CategoryOption[]>(this.base); }
  list(): Observable<AdminCategoryRow[]> { return this.http.get<AdminCategoryRow[]>(`${this.base}/manage`); }
  get(id: number): Observable<AdminCategoryDetail> { return this.http.get<AdminCategoryDetail>(`${this.base}/${id}`); }
  create(input: CategoryInput): Observable<{ id: number }> { return this.http.post<{ id: number }>(this.base, input); }
  update(id: number, input: CategoryInput): Observable<void> { return this.http.put<void>(`${this.base}/${id}`, input); }
  delete(id: number): Observable<void> { return this.http.delete<void>(`${this.base}/${id}`); }
}
