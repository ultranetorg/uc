import { FieldType } from "./FieldType.ts"
import { TokenType } from "./TokenType.ts"

export type ProductField = {
  name: TokenType
  type?: FieldType
  value: unknown
  children?: ProductField[]
}
