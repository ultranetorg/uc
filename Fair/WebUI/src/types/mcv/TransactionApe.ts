import { TransactionStatus } from "types"

export type TransactionApe = {
  id: string
  nid: number
  tag: string
  status: TransactionStatus
}
