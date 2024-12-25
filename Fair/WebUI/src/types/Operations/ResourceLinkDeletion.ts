import { ResourceAddress } from "types"

import { Operation } from "./Operation"

export type ResourceLinkDeletion = {
  source: ResourceAddress
  destination: ResourceAddress
} & Operation
