import { TField } from "./base/Field.ts"
import { TToken } from "./base/Token.ts"

export interface ProductFieldValueMetadata {
  name: TToken
  type: TField
}

export interface ProductFieldBase<TModel> {
  name: TToken
  type?: TField
  metadata?: ProductFieldValueMetadata[]
  value: string
  children?: TModel[]
}

export type ProductFieldModel = ProductFieldBase<ProductFieldModel>

export interface ProductFieldCompare {
  from: ProductFieldModel[];
  to: ProductFieldModel[];
}

export interface ProductFieldViewModel extends ProductFieldBase<ProductFieldViewModel> {
  id: string
  parent?: ProductFieldViewModel
}

export interface ProductFieldCompareViewModel extends ProductFieldBase<ProductFieldCompareViewModel> {
  id: string
  parent?: ProductFieldCompareViewModel
  isRemoved?: true
  isAdded?: true
  isChanged?: true
  oldValue?: string
}
