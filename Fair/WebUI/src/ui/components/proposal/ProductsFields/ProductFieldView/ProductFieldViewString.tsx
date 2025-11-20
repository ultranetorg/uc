import { memo } from "react"
import { ProductFieldViewProp } from "./types"

function getAdded(value: unknown) {
  return <div className="text-green-700">{value as string}</div>
}
function getRemoved(value: unknown) {
  return <div className="text-red-500 line-through opacity-75">{value as string}</div>
}

export const ProductFieldViewString = memo(({ value, oldValue, status }: ProductFieldViewProp) => {
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
      return <div>{value as string}</div>
    }
  }
})
