import { FieldType } from "./FieldType.ts"
import { TokenType } from "./TokenType.ts"

export interface ProductFieldBase<TModel> {
  name: TokenType
  type?: FieldType
  value: string
  children?: TModel[]
}

export type ProductFieldModel = ProductFieldBase<ProductFieldModel>

export interface ProductFieldCompare {
  from: ProductFieldModel[]
  to: ProductFieldModel[]
}

export interface ProductFieldViewModel extends ProductFieldBase<ProductFieldViewModel> {
  id: string
  parent?: ProductFieldViewModel
}
