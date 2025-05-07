import { PaginationResponse, PaginationResult } from "types"

export const toPaginationResponse = async <T>(response: Response): Promise<PaginationResponse<T>> => {
  const itemsResponse = (await response.json()) as T[]
  const totalItems = parseInt(response.headers.get("x-total-items")!)
  const page = parseInt(response.headers.get("x-page")!)
  const pageSize = parseInt(response.headers.get("x-page-size")!)

  return {
    items: itemsResponse,
    totalItems,
    page,
    pageSize,
  }
}

export const toPaginationResult = async <T>(response: Response): Promise<PaginationResult<T>> => {
  const itemsResponse = (await response.json()) as T[]
  const page = parseInt(response.headers.get("x-page")!)
  const pageSize = parseInt(response.headers.get("x-page-size")!)

  return {
    items: itemsResponse,
    page,
    pageSize,
  }
}
