import { ProductSearchResultBase } from "./ProductSearchResultBase"
import { StorePublication } from "./StorePublication"

export type ProductSearchResult = {
  sitesPublications: StorePublication[]
} & ProductSearchResultBase
