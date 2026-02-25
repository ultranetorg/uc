import { UseControllerProps } from "react-hook-form"

import { AccountBase, AuthorBaseAvatar, CreateProposalData, ExtendedOperationType } from "types"

export type FieldValueType =
  | "authors-additions"
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
  | "authorsIds"
  | "candidatesAccounts"
  | "categoryId"
  | "categoryTitle"
  | "description"
  | "fileId"
  | "moderatorsIds"
  | "name"
  | "parentCategoryId"
  | "siteTitle"
  | "slogan"
  | "status"
  | "type"
  | "version"

export type AuthorAvatar = Omit<AuthorBaseAvatar, "owners">

export type EditorField = {
  valueType?: FieldValueType
  name: FieldNameType
  placeholder?: string
  rules?: UseControllerProps<CreateProposalData>["rules"]
}

export type EditorFieldRendererParams = {
  errorMessage?: string
  field: EditorField
  value: string | string[] | AccountBase[] | AuthorAvatar[]
  onChange: (value?: string | string[] | AccountBase[] | AuthorAvatar) => void
}

export type EditorFieldRenderer = (params: EditorFieldRendererParams) => JSX.Element

export type ParameterValueType = "category" | "product" | "publication" | "review" | "user"

export type ParameterNameType = "categoryId" | "productId" | "publicationId" | "reviewId" | "userId"

export type EditorOperationFields = {
  operationType: ExtendedOperationType
  parameterValueType?: ParameterValueType
  parameterName?: ParameterNameType
  parameterLabel?: string
  parameterPlaceholder?: string
  parameterRules?: UseControllerProps<CreateProposalData>["rules"]
  fields?: EditorField[]
}
