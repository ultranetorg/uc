import { Api, PaginationResponse, Product } from "types"

import { toPaginationResponse } from "./utils"

const { VITE_APP_API_BASE_URL: BASE_URL } = import.meta.env

const getProduct = (id: string): Promise<Product> => {
  return fetch(`${BASE_URL}/products/${id}`).then(res => res.json())
}

const getUrlParams = (name?: string, page?: number, pageSize?: number): URLSearchParams => {
  const params = new URLSearchParams()

  if (name !== null && name !== undefined && name != "") {
    params.append("name", name)
  }
  if (page !== undefined) {
    params.append("page", page.toString())
  }
  if (pageSize !== undefined) {
    params.append("pageSize", pageSize.toString())
  }

  return params
}

const getProducts = async (name?: string, page?: number, pageSize?: number): Promise<PaginationResponse<Product>> => {
  const params = getUrlParams(name, page, pageSize)
  const res = await fetch(`${BASE_URL}/products` + (params.size > 0 ? `?${params.toString()}` : ""))
  return await toPaginationResponse(res)
}

const api: Api = {
  getProduct,
  getProducts,
}

export const getApi = () => api
