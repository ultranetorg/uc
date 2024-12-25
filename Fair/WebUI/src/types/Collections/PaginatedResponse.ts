import { ChildItemsArray } from "./ChildItemsArray"

export type PaginatedResponse<T> = ChildItemsArray<T> & {
  page: number
  pageSize: number
}
