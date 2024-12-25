import { ResourceAddress, ResourceData } from "types"

import { Operation } from "./Operation"

export type ResourceUpdation = {
  resource: ResourceAddress
  changes: string[]
  flags: string[]
  data?: ResourceData
  parent?: string
} & Operation
