import { memo } from "react"
import { base64ToUtf8String } from "utils"
import { ProductFieldViewProps } from "./product-field-view-props.ts"

function getAdded(value: string) {
  return <div className="text-green-700">{base64ToUtf8String(value!)}</div>
}
function getRemoved(value: string) {
  return <div className="text-red-500 line-through opacity-75">{base64ToUtf8String(value!)}</div>
}

export const ProductFieldViewString = memo(({ value, oldValue, status }: ProductFieldViewProps) => {
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
      return <div>{base64ToUtf8String(value)}</div>
    }
  }
})
