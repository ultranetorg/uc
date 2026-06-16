import { FieldValue } from "./FieldValue"
import { PublicationChanged } from "./PublicationChanged"

export type PublicationChangedDetails = {
  rating: number
  fields: FieldValue[]
  fieldsTo: FieldValue[]
} & PublicationChanged
