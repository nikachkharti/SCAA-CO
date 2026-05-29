export type SortDirection = 'asc' | 'desc';

export interface PagedRequest {
  pageNumber: number;
  pageSize: number;
  sortBy?: string;
  ascending?: boolean;
}

export interface PagedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
}
