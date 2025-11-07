import { memo } from "react"
import { base64ToBigInt } from "utils"
import { ProductFieldViewProps } from "./product-field-view-props.ts"

function getAdded(value: string) {
  return <div className="text-green-700">{String(base64ToBigInt(value))}</div>
}
function getRemoved(value: string) {
  return <div className="text-red-500 line-through opacity-75">{String(base64ToBigInt(value))}</div>
}

export const ProductFieldViewBigInt = memo(({ value, oldValue, status }: ProductFieldViewProps) => {
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
      return <div>{String(base64ToBigInt(value))}</div>
    }
  }
})
