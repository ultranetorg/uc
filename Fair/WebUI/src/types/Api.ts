import { PaginationResponse, Product } from "types"

export type Api = {
  getProduct(id: string): Promise<Product>
  getProducts(name?: string, page?: number, pageSize?: number): Promise<PaginationResponse<Product>>
}
