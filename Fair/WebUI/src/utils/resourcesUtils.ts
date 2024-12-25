import { ResourceAddress } from "types"

export const parseAddress = (resource: string): ResourceAddress | undefined => {
  if (resource.length < 3) {
    return undefined
  }

  const index = resource.indexOf("/")
  return index !== -1 && index !== resource.length - 1
    ? { author: resource.substring(0, index), resource: resource.substring(index + 1)! }
    : undefined
}
