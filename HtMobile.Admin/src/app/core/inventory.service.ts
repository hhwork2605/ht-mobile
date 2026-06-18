import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { StoreRow, StoreInput, StockRow, StockUpdateItem } from './inventory.models';

@Injectable({ providedIn: 'root' })
export class InventoryService {
  private base = `${environment.apiBase}/stores`;

  constructor(private http: HttpClient) {}

  stores(): Observable<StoreRow[]> { return this.http.get<StoreRow[]>(this.base); }
  createStore(input: StoreInput): Observable<{ id: number }> { return this.http.post<{ id: number }>(this.base, input); }
  updateStore(id: number, input: StoreInput): Observable<void> { return this.http.put<void>(`${this.base}/${id}`, input); }
  deleteStore(id: number): Observable<void> { return this.http.delete<void>(`${this.base}/${id}`); }

  stock(storeId: number): Observable<StockRow[]> { return this.http.get<StockRow[]>(`${this.base}/${storeId}/stock`); }
  setStock(storeId: number, items: StockUpdateItem[]): Observable<void> {
    return this.http.put<void>(`${this.base}/${storeId}/stock`, { items });
  }
}
