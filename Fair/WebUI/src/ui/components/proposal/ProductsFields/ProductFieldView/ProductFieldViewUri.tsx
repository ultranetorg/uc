import { memo } from "react"
import { base64ToUtf8String } from "utils"
import { Link } from "react-router-dom"
import { ProductFieldViewProps } from "./product-field-view-props.ts"

function ensureProtocol(uri: string) {
  if (/^(https?:)?\/\//i.test(uri)) return uri
  return `https://${uri}`
}

function getAdded(value: string) {
  const rawUri = base64ToUtf8String(value)
  const uri = ensureProtocol(rawUri)

  return (
    <Link
      to={uri}
      target="_blank"
      className="text-green-700 underline transition-colors duration-150 hover:text-blue-800 hover:underline"
    >
      {rawUri}
    </Link>
  )
}
function getRemoved(value: string) {
  const rawUri = base64ToUtf8String(value)
  const uri = ensureProtocol(rawUri)

  return (
    <Link
      to={uri}
      target="_blank"
      className="text-red-500 underline line-through opacity-75 transition-colors duration-150 hover:text-blue-800 hover:underline"
    >
      {rawUri}
    </Link>
  )
}

function getNew(value: string) {
  const rawUri = base64ToUtf8String(value)
  const uri = ensureProtocol(rawUri)

  return (
    <Link
      to={uri}
      target="_blank"
      className="text-blue-600 underline transition-colors duration-150 hover:text-blue-800 hover:underline"
    >
      {rawUri}
    </Link>
  )
}

export const ProductFieldViewUri = memo(({ value, oldValue, status }: ProductFieldViewProps) => {
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
      return getNew(value)
    }
  }
})
