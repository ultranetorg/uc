import { ResourceAddress } from "./ResourceAddress"
import { ResourceData } from "./ResourceData"

export type ResourceInfo = {
  id: string
  address: ResourceAddress
  data?: ResourceData

  author: string
}
