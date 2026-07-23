import { BaseVotableOperation } from "./BaseVotableOperation"

export type StoreInfoUpdation = {
  title: string
  slogan: string
  description: string
} & BaseVotableOperation
