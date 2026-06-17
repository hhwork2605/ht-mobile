import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { UserRow, CreateUserRequest } from './user.models';

@Injectable({ providedIn: 'root' })
export class UsersService {
  private base = `${environment.apiBase}/users`;

  constructor(private http: HttpClient) {}

  list(): Observable<UserRow[]> { return this.http.get<UserRow[]>(this.base); }
  roles(): Observable<string[]> { return this.http.get<string[]>(`${this.base}/roles`); }
  create(req: CreateUserRequest): Observable<{ id: number }> { return this.http.post<{ id: number }>(this.base, req); }
  setRoles(id: number, roles: string[]): Observable<void> { return this.http.put<void>(`${this.base}/${id}/roles`, { roles }); }
  lock(id: number): Observable<void> { return this.http.post<void>(`${this.base}/${id}/lock`, {}); }
  unlock(id: number): Observable<void> { return this.http.post<void>(`${this.base}/${id}/unlock`, {}); }
}
