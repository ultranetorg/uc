import { FieldValue } from "./ProductField"
import { PublicationDetails } from "./PublicationDetails"

export type PublicationDetailsDiff = {
  fieldsTo: FieldValue[]
} & PublicationDetails
