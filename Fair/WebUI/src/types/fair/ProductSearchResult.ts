import { ProductSearchResultBase } from "./ProductSearchResultBase"
import { SitePublication } from "./SitePublication"

export type ProductSearchResult = {
  sitesPublications: SitePublication[]
} & ProductSearchResultBase
