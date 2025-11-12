import { memo } from "react"
import { Link } from "react-router-dom"

import { Types } from "./types"

function ensureProtocol(uri: string) {
  if (/^(https?:)?\/\//i.test(uri)) return uri
  return `https://${uri}`
}

function getAdded(value: string) {
  const uri = ensureProtocol(value)

  return (
    <Link
      to={uri}
      target="_blank"
      className="text-green-700 underline transition-colors duration-150 hover:text-blue-800 hover:underline"
    >
      {value}
    </Link>
  )
}
function getRemoved(value: string) {
  const uri = ensureProtocol(value)

  return (
    <Link
      to={uri}
      target="_blank"
      className="text-red-500 underline line-through opacity-75 transition-colors duration-150 hover:text-blue-800 hover:underline"
    >
      {value}
    </Link>
  )
}

function getNew(value: string) {
  const uri = ensureProtocol(value)

  return (
    <Link
      to={uri}
      target="_blank"
      className="text-blue-600 underline transition-colors duration-150 hover:text-blue-800 hover:underline"
    >
      {value}
    </Link>
  )
}

export const ProductFieldViewUri = memo(({ value, oldValue, status }: Types) => {
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
