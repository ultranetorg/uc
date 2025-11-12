import { CompareStatus } from "../types"

export interface Types {
  value: string
  oldValue: string | null
  status: CompareStatus
}
