import { OperationType } from "types"

export type FieldValueType =
  | "approval-policy"
  | "authors-array"
  | "category"
  | "category-type"
  | "file"
  | "operation-class"
  | "moderators-array"
  | "review-status"
  | "roles"
  | "string"
  | "string-multiline"
  | "version"

export type ParameterValueType = "category" | "product" | "publication" | "review" | "user"

export type EditorField = {
  valueType?: FieldValueType
  name: string
  placeholder?: string
}

export type EditorOperationFields = {
  operationType: OperationType
  parameterValueType?: ParameterValueType
  parameterName?: string
  parameterLabel?: string
  parameterPlaceholder?: string
  fields?: EditorField[]
}
