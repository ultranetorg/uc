import { PaginationResult } from "./PaginationResult"

export type TotalItemsResult<T> = {
  totalItems: number
} & PaginationResult<T>
