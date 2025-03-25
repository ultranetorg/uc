export type PaginationResponse<T> = {
  items: T[]
  totalItems: number
  page: number
  pageSize: number
}
