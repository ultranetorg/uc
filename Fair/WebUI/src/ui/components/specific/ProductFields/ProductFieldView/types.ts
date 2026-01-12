import { CompareStatus } from "../models"

export interface ProductFieldViewProp {
  value: unknown
  oldValue?: unknown
  status: CompareStatus
}
