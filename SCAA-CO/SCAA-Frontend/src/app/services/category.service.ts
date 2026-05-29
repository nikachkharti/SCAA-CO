import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';

import { environment } from '../../environments/environment';
import {
  CategoryForCreatingDto,
  CategoryForGettingDto,
  CategoryForUpdatingDto
} from '../models/category.model';
import { CommonResponse } from '../models/common-response.model';

@Injectable({ providedIn: 'root' })
export class CategoryService {
  private readonly http = inject(HttpClient);
  private readonly endpoint = `${environment.apiBaseUrl}/categories`;

  getAll(): Observable<CategoryForGettingDto[]> {
    return this.http
      .get<CommonResponse<CategoryForGettingDto[]>>(this.endpoint)
      .pipe(map(response => response.result));
  }

  getById(id: number): Observable<CategoryForGettingDto> {
    return this.http
      .get<CommonResponse<CategoryForGettingDto>>(`${this.endpoint}/${id}`)
      .pipe(map(response => response.result));
  }

  create(model: CategoryForCreatingDto): Observable<CategoryForGettingDto> {
    return this.http
      .post<CommonResponse<number>>(this.endpoint, model)
      .pipe(map(response => ({ id: response.result, categoryName: model.categoryName })));
  }

  update(model: CategoryForUpdatingDto): Observable<CategoryForGettingDto> {
    return this.http
      .put<CommonResponse<number>>(this.endpoint, model)
      .pipe(map(() => ({ id: model.id, categoryName: model.categoryName })));
  }

  delete(id: number): Observable<void> {
    return this.http
      .delete<CommonResponse<null>>(`${this.endpoint}/${id}`)
      .pipe(map(() => void 0));
  }
}
