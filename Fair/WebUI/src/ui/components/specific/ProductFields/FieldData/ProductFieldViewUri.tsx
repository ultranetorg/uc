import { memo } from "react"
import { Link } from "react-router-dom"

import { ensureHttp } from "utils"

import { ProductFieldViewProp } from "./types"

function getAdded(value: unknown) {
  const uri = ensureHttp(value as string)

  return (
    <Link
      to={uri}
      target="_blank"
      className="text-green-700 underline transition-colors duration-150 hover:text-blue-800 hover:underline"
    >
      {uri}
    </Link>
  )
}
function getRemoved(value: unknown) {
  const uri = ensureHttp(value as string)

  return (
    <Link
      to={uri}
      target="_blank"
      className="text-red-500 underline opacity-75 transition-colors duration-150 hover:text-blue-800 hover:underline"
    >
      {uri}
    </Link>
  )
}

function getNew(value: unknown) {
  const uri = ensureHttp(value as string)

  return (
    <Link
      to={uri}
      target="_blank"
      className="text-blue-600 underline transition-colors duration-150 hover:text-blue-800 hover:underline"
    >
      {uri}
    </Link>
  )
}

export const ProductFieldViewUri = memo(({ value, oldValue, status }: ProductFieldViewProp) => {
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
      return getNew(value)
    }
  }
})
