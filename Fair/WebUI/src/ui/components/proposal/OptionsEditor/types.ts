import { UseControllerProps } from "react-hook-form"

import { UserBase, AuthorBaseAvatar, CreateProposalData, OperationType } from "types"

export type FieldValueType =
  | "authors-removals"
  | "category"
  | "category-root"
  | "category-type"
  | "file"
  | "moderators-additions"
  | "moderators-removals"
  | "review-status"
  | "string"
  | "string-multiline"
  | "version"

export type FieldNameType =
  | "authors"
  | "categoryId"
  | "categoryTitle"
  | "description"
  | "fileId"
  | "moderators"
  | "name"
  | "parentCategoryId"
  | "storeTitle"
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
  value: string | string[] | UserBase[] | AuthorBaseAvatar[]
  onChange: (value?: string | string[] | UserBase[] | AuthorBaseAvatar[] | null) => void
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
  parameterHasRoot?: boolean
  fields?: EditorField[]
}
