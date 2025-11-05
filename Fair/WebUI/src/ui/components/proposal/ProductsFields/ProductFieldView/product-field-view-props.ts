import { CompareStatus } from "../selected-props.ts"

export interface ProductFieldViewProps {
  value: string
  oldValue: string | null
  status: CompareStatus
}
