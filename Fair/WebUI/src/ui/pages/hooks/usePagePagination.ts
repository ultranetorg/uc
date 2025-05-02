import { useCallback, useEffect, useState } from "react"
import { useSearchParams } from "react-router-dom"

import { DEFAULT_PAGE_SIZE_2 } from "constants"
import { isInteger } from "utils"

const getSearchParams = (searchParams: URLSearchParams) => {
  const pageParam = searchParams.get("page") ?? searchParams.get("p")
  const sizeParam = searchParams.get("pageSize") ?? searchParams.get("s")

  const page = isInteger(pageParam) ? parseInt(pageParam!) : 0
  const pageSize = isInteger(sizeParam) ? parseInt(sizeParam!) : DEFAULT_PAGE_SIZE_2
  const search = searchParams.get("search") ?? searchParams.get("q") ?? ""

  return { page, pageSize, search }
}

// TODO: refactor this hook.
export const usePagePagination = () => {
  const [page, setPage] = useState(0)
  const [pageSize, setPageSize] = useState(DEFAULT_PAGE_SIZE_2)
  const [search, setSearch] = useState("")

  const [searchParams, setSearchParams] = useSearchParams()

  useEffect(() => {
    const params = getSearchParams(searchParams)
    if (params.page !== page) {
      setPage(params.page)
    }
    if (params.pageSize !== pageSize) {
      setPageSize(params.pageSize)
    }
    if (params.search !== search) {
      setSearch(params.search)
    }
  }, [])

  useEffect(() => {
    if (page !== 0) {
      searchParams.set("page", page.toString())
    } else {
      searchParams.delete("page")
    }
    if (pageSize !== DEFAULT_PAGE_SIZE_2) {
      searchParams.set("pageSize", pageSize.toString())
    } else {
      searchParams.delete("pageSize")
    }
    if (search !== "") {
      searchParams.set("search", search)
    } else {
      searchParams.delete("search")
    }
    setSearchParams(searchParams)
  }, [page, pageSize, search, searchParams, setSearchParams])

  const resetPagination = useCallback(() => {
    setPage(0)
    setPageSize(DEFAULT_PAGE_SIZE_2)
    setSearch("")
  }, [])

  return {
    page,
    setPage,
    pageSize,
    setPageSize,
    search,
    setSearch,
    resetPagination,
  }
}
