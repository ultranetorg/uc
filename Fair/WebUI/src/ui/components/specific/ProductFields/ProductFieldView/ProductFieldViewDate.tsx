import { memo } from "react"
import { ProductFieldViewProp } from "./types"
import { formatSecDate } from "utils"

function getAdded(value: unknown) {
  return <div className="text-green-700">{formatSecDate(Number(value))}</div>
}
function getRemoved(value: unknown) {
  return <div className="text-red-500 line-through opacity-75">{formatSecDate(Number(value))}</div>
}

export const ProductFieldViewDate = memo(({ value, oldValue, status }: ProductFieldViewProp) => {
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
      return <div>{formatSecDate(Number(value))}</div>
    }
  }
})
