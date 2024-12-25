import { useCallback } from "react"
import { useSearchParams } from "react-router-dom"

const getValue = (name: string, searchParams: URLSearchParams): number => {
  const val = searchParams.get(name)
  return val && +val >= 0 ? +val : 0
}

export const useUrlPagination = () => {
  const [searchParams, setSearchParams] = useSearchParams()

  const setPage = useCallback(
    (page: number) => {
      if (page >= 0) {
        searchParams.set("page", page.toString())
        setSearchParams(searchParams, { replace: true })
      }
    },
    [searchParams, setSearchParams],
  )

  const setPageSize = useCallback(
    (pageSize: number) => {
      if (pageSize >= 0) {
        searchParams.set("pageSize", pageSize.toString())
        setSearchParams(searchParams, { replace: true })
      }
    },
    [searchParams, setSearchParams],
  )

  return {
    page: getValue("page", searchParams),
    pageSize: getValue("pageSize", searchParams),
    setPage,
    setPageSize,
  }
}
