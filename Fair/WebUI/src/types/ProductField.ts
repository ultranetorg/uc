import { FieldType } from "./FieldType.ts"
import { TokenType } from "./TokenType.ts"

export type FieldValue = {
  name: TokenType
  type?: FieldType
  value: unknown
  children?: FieldValue[]
}
