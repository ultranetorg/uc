import { AxiosResponseHeaders, RawAxiosResponseHeaders } from "axios"

import { Auctions, PaginatedResponse } from "types"

export const toPaginatedResponse = <T>(
  data: T[],
  headers: RawAxiosResponseHeaders | AxiosResponseHeaders,
): PaginatedResponse<T> => {
  const page = parseInt(headers["x-page"]!)
  const pageSize = parseInt(headers["x-page-size"]!)
  const totalItems = parseInt(headers["x-total-items"]!)

  return {
    items: data,
    page,
    pageSize,
    totalItems,
  }
}

export const toAuctions = (data: any, headers: RawAxiosResponseHeaders | AxiosResponseHeaders): Auctions => {
  const page = parseInt(headers["x-page"]!)
  const pageSize = parseInt(headers["x-page-size"]!)
  const totalItems = parseInt(headers["x-total-items"]!)

  return {
    ...data,
    page,
    pageSize,
    totalItems,
  }
}
