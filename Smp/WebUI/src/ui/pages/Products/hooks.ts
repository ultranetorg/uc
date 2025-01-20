import { DEFAULT_PAGE_SIZE } from "constants"
import { SetURLSearchParams, useSearchParams } from "react-router-dom"

import { isInteger } from "utils"

type Result = {
  setSearchParams: SetURLSearchParams
  name?: string
  page?: number
  pageSize?: number
}

export const useQueryParams = (): Result => {
  const [searchParams, setSearchParams] = useSearchParams()

  const name = searchParams.get("name") ?? undefined
  const pageParam = searchParams.get("page")
  const sizeParam = searchParams.get("pageSize")

  const page = isInteger(pageParam) ? parseInt(pageParam!) : 0
  const pageSize = isInteger(sizeParam) ? parseInt(sizeParam!) : DEFAULT_PAGE_SIZE

  return {
    setSearchParams,
    name,
    page: page !== 0 ? page : undefined,
    pageSize: pageSize !== DEFAULT_PAGE_SIZE ? pageSize : undefined,
  }
}
