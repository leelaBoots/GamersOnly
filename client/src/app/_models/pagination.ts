export interface Pagination {
  currentPage: number;
  itemsPerPage: number;
  totalItems: number;
  totalPages: number;
}

// type T will effectively be an array of Members (Member[])
export class PaginatedResult<T> {
  result: T;
  pagination: Pagination;
}