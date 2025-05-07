import { TotalItemsResult, PaginationResult } from "types"

type ParamsValue = string | number | undefined

type FilterRules<T extends Record<string, ParamsValue>> = {
  [K in keyof T]?: (value: T[K]) => boolean
}

export const buildUrlParams = <T extends Record<string, ParamsValue>>(
  paramsObj: T,
  filters: FilterRules<T> = {},
): string => {
  const params = new URLSearchParams()

  for (const [key, value] of Object.entries(paramsObj) as [keyof T, T[keyof T]][]) {
    const filterFn = filters[key]
    const shouldInclude =
      value !== undefined &&
      value !== "" &&
      !(typeof value === "number" && isNaN(value)) &&
      (filterFn ? filterFn(value) : true)

    if (shouldInclude) {
      params.append(String(key), String(value))
    }
  }

  return params.size > 0 ? `?${params.toString()}` : ""
}

export const toTotalItemsResult = async <T>(response: Response): Promise<TotalItemsResult<T>> => {
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
