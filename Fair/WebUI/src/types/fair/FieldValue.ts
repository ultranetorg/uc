import { FieldType } from "./FieldType"
import { TokenType } from "./TokenType"

export type FieldValue = {
  name: TokenType
  type?: FieldType
  value: unknown
  children?: FieldValue[]
}
