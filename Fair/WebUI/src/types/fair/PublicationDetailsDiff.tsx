import { FieldValue } from "./FieldValue"
import { PublicationDetails } from "./PublicationDetails"

export type PublicationDetailsDiff = {
  fieldsTo: FieldValue[]
} & PublicationDetails
