import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { SalesReport } from './report.models';

@Injectable({ providedIn: 'root' })
export class ReportsService {
  constructor(private http: HttpClient) {}

  /** from/to dạng yyyy-MM-dd (date-only) để tránh lệch múi giờ. */
  sales(from: string, to: string): Observable<SalesReport> {
    const params = new HttpParams().set('from', from).set('to', to);
    return this.http.get<SalesReport>(`${environment.apiBase}/reports/sales`, { params });
  }
}
