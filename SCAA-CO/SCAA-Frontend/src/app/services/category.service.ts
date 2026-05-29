import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';

import { environment } from '../../environments/environment';
import {
  CategoryForCreatingDto,
  CategoryForGettingDto,
  CategoryForUpdatingDto
} from '../models/category.model';
import { CommonResponse } from '../models/common-response.model';
import { PagedRequest, PagedResponse } from '../models/paged.model';

@Injectable({ providedIn: 'root' })
export class CategoryService {
  private readonly http = inject(HttpClient);
  private readonly endpoint = `${environment.apiBaseUrl}/categories`;

  getAll(
    request: PagedRequest = { pageNumber: 1, pageSize: 10 }
  ): Observable<PagedResponse<CategoryForGettingDto>> {
    let params = new HttpParams()
      .set('PageNumber', request.pageNumber)
      .set('PageSize', request.pageSize);

    if (request.sortBy) {
      params = params.set('SortBy', request.sortBy);
    }
    if (request.ascending !== undefined) {
      params = params.set('Ascending', request.ascending);
    }

    return this.http
      .get<CommonResponse<PagedResponse<CategoryForGettingDto>>>(this.endpoint, { params })
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
