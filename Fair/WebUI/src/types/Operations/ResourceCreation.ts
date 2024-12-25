import { ResourceAddress, ResourceData } from "types"

import { Operation } from "./Operation"

export type ResourceCreation = {
  resource: ResourceAddress
  flags: string[]
  data?: ResourceData
  parent?: string
} & Operation
