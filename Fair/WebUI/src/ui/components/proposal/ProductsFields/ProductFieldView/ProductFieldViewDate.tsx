import { memo } from "react"
import { ProductFieldViewProp } from "./types"
import { START_DATE } from "config"

function formatValue(value: unknown) {
  const { days } = value as { days: number }
  const date = new Date(START_DATE)
  date.setUTCDate(date.getUTCDate() + days)
  return date.toISOString()
}

function getAdded(value: unknown) {
  return <div className="text-green-700">{formatValue(value)}</div>
}
function getRemoved(value: unknown) {
  return <div className="text-red-500 line-through opacity-75">{formatValue(value)}</div>
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
          {getRemoved(oldValue!)}
          {getAdded(value)}
        </div>
      )
    }
    default: {
      return <div>{formatValue(value)}</div>
    }
  }
})
