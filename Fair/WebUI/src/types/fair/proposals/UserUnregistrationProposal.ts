import { Proposal } from "./Proposal"

export type UserUnregistrationProposal = {
  userId: string
  userName: string
} & Proposal
