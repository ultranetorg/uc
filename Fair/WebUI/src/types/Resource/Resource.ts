import { ChildItemsArray } from "types/Collections"

import { ResourceAddress } from "./ResourceAddress"
import { ResourceData } from "./ResourceData"
import { ResourceLink } from "./ResourceLink"

export type Resource = {
  id: number
  address: ResourceAddress
  flags: string
  data?: ResourceData
  updated: number

  inbounds: ChildItemsArray<ResourceLink>
  outbounds: ChildItemsArray<ResourceLink>
}
