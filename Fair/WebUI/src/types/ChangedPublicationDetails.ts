import { ChangedPublication } from "./ChangedPublication"
import { ProductField } from "./ProductField"

export type ChangedPublicationDetails = {
  from: ProductField[]
  to: ProductField[]
} & ChangedPublication
