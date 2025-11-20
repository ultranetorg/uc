import { CompareStatus } from "../types"

export interface ProductFieldViewProp {
  value: unknown
  oldValue: unknown | null
  status: CompareStatus
}
