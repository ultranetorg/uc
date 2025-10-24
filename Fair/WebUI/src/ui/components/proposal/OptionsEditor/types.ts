import { UseControllerProps } from "react-hook-form"

import { AccountBase, CreateProposalData, OperationType } from "types"

export type FieldValueType =
  | "authors-additions"
  | "authors-removals"
  | "category"
  | "category-type"
  | "file"
  | "moderators-additions"
  | "moderators-removals"
  | "review-status"
  | "string"
  | "string-multiline"
  | "version"

export type FieldNameType =
  | "authorsIds"
  | "candidatesIds"
  | "categoryId"
  | "categoryTitle"
  | "description"
  | "fileId"
  | "moderatorsIds"
  | "nickname"
  | "parentCategoryId"
  | "siteTitle"
  | "slogan"
  | "status"
  | "type"
  | "version"

export type EditorField = {
  valueType?: FieldValueType
  name: FieldNameType
  placeholder?: string
  rules?: UseControllerProps<CreateProposalData>["rules"]
}

export type EditorFieldRendererParams = {
  errorMessage?: string
  field: EditorField
  value: string | string[] | AccountBase[]
  onChange: (value: string | string[] | AccountBase[]) => void
}

export type EditorFieldRenderer = (params: EditorFieldRendererParams) => JSX.Element

export type ParameterValueType = "category" | "product" | "publication" | "review" | "user"

export type ParameterNameType = "categoryId" | "productId" | "publicationId" | "reviewId" | "userId"

export type EditorOperationFields = {
  operationType: OperationType
  parameterValueType?: ParameterValueType
  parameterName?: ParameterNameType
  parameterLabel?: string
  parameterPlaceholder?: string
  parameterRules?: UseControllerProps<CreateProposalData>["rules"]
  fields?: EditorField[]
}
