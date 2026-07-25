import { StoreBase } from "./StoreBase"

export type Store = {
  authorsIds: string[]
  moderatorsIds: string[]
} & StoreBase
