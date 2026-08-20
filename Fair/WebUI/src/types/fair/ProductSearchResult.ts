import { ProductPublication } from "./ProductPublication"
import { ProductType } from "./ProductType"

export type ProductSearchResult = {
  productId: string
  productLogoId?: string
  productTitle: string
  productType: ProductType
  authorTitle: string
  publications: ProductPublication[]
  hasMorePublications: boolean
}
