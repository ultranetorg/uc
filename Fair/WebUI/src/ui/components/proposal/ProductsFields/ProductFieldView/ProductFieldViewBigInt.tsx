import { memo } from "react"

import { Types } from "./types"

function getAdded(value: string) {
  return <div className="text-green-700">{value}</div>
}
function getRemoved(value: string) {
  return <div className="text-red-500 line-through opacity-75">{value}</div>
}

export const ProductFieldViewBigInt = memo(({ value, oldValue, status }: Types) => {
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
          {getRemoved(oldValue!)}
          {getAdded(value)}
        </div>
      )
    }
    default: {
      return <div>{value}</div>
    }
  }
})
