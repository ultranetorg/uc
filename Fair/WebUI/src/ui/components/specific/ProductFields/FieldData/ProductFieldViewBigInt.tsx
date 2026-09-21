import { memo } from "react"

import { ProductFieldViewProp } from "./types"
import { getAdded, getRemoved } from "./utils"

export const ProductFieldViewBigInt = memo(({ value, oldValue, status }: ProductFieldViewProp) => {
  switch (status) {
    case "added": {
      return getAdded(value)
    }
    case "removed": {
      return getRemoved(oldValue ?? value)
    }
    case "changed": {
      return (
        <div>
          {getRemoved(oldValue ?? value)}
          {getAdded(value)}
        </div>
      )
    }
    default: {
      return <div>{value as number}</div>
    }
  }
})
