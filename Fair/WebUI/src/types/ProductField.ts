import { TField } from "./base/Field.ts"
import { TToken } from "./base/Token.ts"

export interface ProductFieldValueMetadataModel {
  name: TToken
  type: TField
}

export interface ProductFieldModelBase<TModel> {
  name: TToken
  type?: TField
  metadata?: ProductFieldValueMetadataModel[]
  value: string
  children?: TModel[]
}

export type ProductFieldModel = ProductFieldModelBase<ProductFieldModel>

export interface ProductFieldCompareModel {
  from: ProductFieldModel[];
  to: ProductFieldModel[];
}

export interface ProductFieldViewModel extends ProductFieldModelBase<ProductFieldViewModel> {
  id: string
  parent?: ProductFieldViewModel
}

export interface ProductFieldCompareViewModel extends ProductFieldModelBase<ProductFieldCompareViewModel> {
  id: string
  parent?: ProductFieldCompareViewModel
  isRemoved?: true
  isAdded?: true
  isChanged?: true
  oldValue?: string
}
