import { TransactionStatus } from "./TransactionStatus"

export type TransactionApe = {
  id: string
  nid: number
  tag: string
  status: TransactionStatus
  error: string | null
}
