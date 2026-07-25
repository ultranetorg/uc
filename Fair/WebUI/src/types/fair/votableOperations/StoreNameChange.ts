import { BaseVotableOperation } from "./BaseVotableOperation"

export type StoreNameChange = {
  storeName: string
  name: string
} & BaseVotableOperation
