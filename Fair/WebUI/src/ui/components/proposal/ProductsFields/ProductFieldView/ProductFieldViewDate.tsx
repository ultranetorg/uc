import { memo } from "react"
import { base64ToUtf8String } from "utils"
import { ProductFieldViewProps } from "./product-field-view-props.ts"

function getAdded(value: string) {
  return <div className="text-green-700">{formatDate(value)}</div>
}
function getRemoved(value: string) {
  return <div className="text-red-500 line-through opacity-75">{formatDate(value)}</div>
}

function formatDate(value: string) {
  const date = new Date(base64ToUtf8String(value))

  return new Intl.DateTimeFormat("en-US", {
    year: "numeric",
    month: "long",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit",
    timeZoneName: "short",
  }).format(date)
}

export const ProductFieldViewDate = memo(({ value, oldValue, status }: ProductFieldViewProps) => {
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
      return <div>{formatDate(value)}</div>
    }
  }
})
