import { memo } from "react"
import { Types } from "./types"
import { formatBase64Date } from "utils"

function getAdded(value: string) {
  return <div className="text-green-700">{formatBase64Date(value)}</div>
}
function getRemoved(value: string) {
  return <div className="text-red-500 line-through opacity-75">{formatBase64Date(value)}</div>
}

export const ProductFieldViewDate = memo(({ value, oldValue, status }: Types) => {
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
      return <div>{formatBase64Date(value)}</div>
    }
  }
})
