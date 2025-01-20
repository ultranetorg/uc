import { Api, Product } from "types"

const { VITE_APP_API_BASE_URL: BASE_URL } = import.meta.env

const getProduct = (id: string): Promise<Product> => {
  return fetch(`${BASE_URL}/products/${id}`).then(res => res.json())
}

const api: Api = {
  getProduct,
}

export const getApi = () => api
