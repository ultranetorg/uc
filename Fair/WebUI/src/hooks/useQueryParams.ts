import { SetURLSearchParams, useSearchParams } from "react-router-dom"

import { DEFAULT_PAGE_SIZE } from "constants"
import { isInteger } from "utils"

type QueryParamsResult = {
  page: number
  pageSize: number
  title: string
  search: string
  setQueryParams: SetURLSearchParams
}

export const useQueryParams = (): QueryParamsResult => {
  const [searchParams, setQueryParams] = useSearchParams()

  const pageParam = searchParams.get("page") ?? searchParams.get("p")
  const sizeParam = searchParams.get("pageSize") ?? searchParams.get("s")

  const page = isInteger(pageParam) ? parseInt(pageParam!) : 0
  const pageSize = isInteger(sizeParam) ? parseInt(sizeParam!) : DEFAULT_PAGE_SIZE
  const title = searchParams.get("title") ?? searchParams.get("t") ?? ""
  const search = searchParams.get("search") ?? searchParams.get("s") ?? ""

  return {
    page,
    pageSize,
    title,
    search,
    setQueryParams,
  }
}
