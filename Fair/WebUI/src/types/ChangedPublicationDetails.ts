import { ChangedPublication } from "./ChangedPublication"
import { FieldValue } from "./ProductField"

export type ChangedPublicationDetails = {
  from: FieldValue[]
  to: FieldValue[]
} & ChangedPublication
