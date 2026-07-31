import { ProductSearchResultBase } from "./ProductSearchResultBase"
import { ProductPublication } from "./ProductPublication"

export type ProductSearchResult = {
  publications: ProductPublication[]
  hasMorePublications: boolean
} & ProductSearchResultBase
