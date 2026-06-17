import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { AdminOrderListDto, AdminOrderDetailDto, OrderStatus } from './order.models';

@Injectable({ providedIn: 'root' })
export class OrdersService {
  private base = `${environment.apiBase}/orders`;

  constructor(private http: HttpClient) {}

  list(status?: OrderStatus | null): Observable<AdminOrderListDto> {
    let params = new HttpParams();
    if (status !== null && status !== undefined) params = params.set('status', status);
    return this.http.get<AdminOrderListDto>(this.base, { params });
  }

  get(id: number): Observable<AdminOrderDetailDto> {
    return this.http.get<AdminOrderDetailDto>(`${this.base}/${id}`);
  }

  changeStatus(id: number, status: OrderStatus): Observable<AdminOrderDetailDto> {
    return this.http.post<AdminOrderDetailDto>(`${this.base}/${id}/status`, { status });
  }
}
