import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { SeoOverview } from './seo.models';

@Injectable({ providedIn: 'root' })
export class SeoService {
  constructor(private http: HttpClient) {}

  overview(): Observable<SeoOverview> {
    return this.http.get<SeoOverview>(`${environment.apiBase}/seo/overview`);
  }
}
