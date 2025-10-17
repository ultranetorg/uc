import { TField } from "./base/Field.ts"
import { TToken } from "./base/Token.ts"

export interface ProductFieldValueMetadataModel {
  name: TToken
  type: TField
}

export interface ProductFieldModel {
  name: TToken
  type?: TField
  metadata?: ProductFieldValueMetadataModel[]
  value: string
  children?: ProductFieldModel[]
}

export interface ProductFieldViewModel extends ProductFieldModel {
  id: string
  parent?: ProductFieldViewModel
  children?: ProductFieldViewModel[]
}
