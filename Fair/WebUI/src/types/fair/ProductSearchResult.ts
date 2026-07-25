import { ProductSearchResultBase } from "./ProductSearchResultBase"
import { StorePublication } from "./StorePublication"

export type ProductSearchResult = {
  storesPublications: StorePublication[]
} & ProductSearchResultBase
