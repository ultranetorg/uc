import { UseControllerProps } from "react-hook-form"

import { CreateProposalData, OperationType } from "types"

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
  rules?: UseControllerProps<CreateProposalData>["rules"]
}

export type EditorFieldRendererParams = {
  errorMessage?: string
  field: EditorField
  value: string
  onChange: (value: string | string[]) => void
}

export type EditorFieldRenderer = (params: EditorFieldRendererParams) => JSX.Element

export type EditorOperationFields = {
  operationType: OperationType
  parameterValueType?: ParameterValueType
  parameterName?: string
  parameterLabel?: string
  parameterPlaceholder?: string
  fields?: EditorField[]
}
