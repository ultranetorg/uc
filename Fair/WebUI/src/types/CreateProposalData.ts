import { OperationType } from "./OperationType"

export type CreateProposalData = Record<string, CreateProposalDataOption[] | string | string[]> & {
  title: string
  description?: string
  duration: string
  type?: OperationType
  options: CreateProposalDataOption[]
}

export type CreateProposalDataOption = Record<string, string | string[]> & {
  title: string
}
