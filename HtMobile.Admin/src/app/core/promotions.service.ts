import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { AdminPromotionRow, AdminPromotionDetail, PromotionInput } from './promotion.models';

@Injectable({ providedIn: 'root' })
export class PromotionsService {
  private base = `${environment.apiBase}/promotions`;

  constructor(private http: HttpClient) {}

  list(): Observable<AdminPromotionRow[]> { return this.http.get<AdminPromotionRow[]>(this.base); }
  get(id: number): Observable<AdminPromotionDetail> { return this.http.get<AdminPromotionDetail>(`${this.base}/${id}`); }
  create(input: PromotionInput): Observable<{ id: number }> { return this.http.post<{ id: number }>(this.base, input); }
  update(id: number, input: PromotionInput): Observable<void> { return this.http.put<void>(`${this.base}/${id}`, input); }
  delete(id: number): Observable<void> { return this.http.delete<void>(`${this.base}/${id}`); }
}
