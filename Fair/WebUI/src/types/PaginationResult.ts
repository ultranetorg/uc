export type PaginationResult<T> = {
  page: number
  pageSize: number
  items: T[]
}
