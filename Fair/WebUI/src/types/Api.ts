import { Product } from "types"

export type Api = {
  getProduct(id: string): Promise<Product>
}
