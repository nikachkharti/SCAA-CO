import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';

import { environment } from '../../environments/environment';
import { CommonResponse } from '../models/common-response.model';
import { PagedRequest, PagedResponse } from '../models/paged.model';
import {
  SupplierForCreatingDto,
  SupplierForGettingDto,
  SupplierForUpdatingDto
} from '../models/supplier.model';

@Injectable({ providedIn: 'root' })
export class SupplierService {
  private readonly http = inject(HttpClient);
  private readonly endpoint = `${environment.apiBaseUrl}/suppliers`;

  getAll(
    request: PagedRequest = { pageNumber: 1, pageSize: 10 }
  ): Observable<PagedResponse<SupplierForGettingDto>> {
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
      .get<CommonResponse<PagedResponse<SupplierForGettingDto>>>(this.endpoint, { params })
      .pipe(map(response => response.result));
  }

  getById(id: number): Observable<SupplierForGettingDto> {
    return this.http
      .get<CommonResponse<SupplierForGettingDto>>(`${this.endpoint}/${id}`)
      .pipe(map(response => response.result));
  }

  create(model: SupplierForCreatingDto): Observable<SupplierForGettingDto> {
    return this.http
      .post<CommonResponse<number>>(this.endpoint, model)
      .pipe(map(response => ({ id: response.result, supplierName: model.supplierName })));
  }

  update(model: SupplierForUpdatingDto): Observable<SupplierForGettingDto> {
    return this.http
      .put<CommonResponse<number>>(this.endpoint, model)
      .pipe(map(() => ({ id: model.id, supplierName: model.supplierName })));
  }

  delete(id: number): Observable<void> {
    return this.http
      .delete<CommonResponse<null>>(`${this.endpoint}/${id}`)
      .pipe(map(() => void 0));
  }
}
