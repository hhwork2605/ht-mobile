import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { DashboardStats } from './dashboard.models';

@Injectable({ providedIn: 'root' })
export class DashboardService {
  constructor(private http: HttpClient) {}

  get(): Observable<DashboardStats> {
    return this.http.get<DashboardStats>(`${environment.apiBase}/dashboard`);
  }
}
