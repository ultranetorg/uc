import { OperationType } from "types"

export type FieldValueType =
  | "approval-policy"
  | "category"
  | "category-type"
  | "file"
  | "operation-class"
  | "review-status"
  | "roles"
  | "string"
  | "string-multiline"
  | "user-array"
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
