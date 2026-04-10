import { ChangedPublication } from "./ChangedPublication"
import { FieldValue } from "./ProductField"

export type ChangedPublicationDetails = {
  rating: number
  fields: FieldValue[]
  fieldsTo: FieldValue[]
} & ChangedPublication
