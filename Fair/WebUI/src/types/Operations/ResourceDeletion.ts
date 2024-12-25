import { ResourceAddress } from "types"

import { Operation } from "./Operation"

export type ResourceDeletion = {
  resource: ResourceAddress
} & Operation
