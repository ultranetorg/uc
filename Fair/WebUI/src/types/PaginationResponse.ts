import { TotalItemsResponse } from "./TotalItemsResponse"

export type PaginationResponse<T> = {
  page: number
  pageSize: number
} & TotalItemsResponse<T>
