import { ResourceAddress } from "types"

import { Operation } from "./Operation"

export type ResourceLinkCreation = {
  source: ResourceAddress
  destination: ResourceAddress
} & Operation
