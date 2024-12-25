import { Operation } from "./Operation"

export type Emission = {
  wei: bigint
  eid: number
} & Operation
