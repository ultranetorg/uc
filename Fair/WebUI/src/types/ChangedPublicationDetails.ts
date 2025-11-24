import { ChangedPublication } from "./ChangedPublication"
import { ProductFieldModel } from "./ProductField"

export type ChangedPublicationDetails = {
  from: ProductFieldModel[]
  to: ProductFieldModel[]
} & ChangedPublication
