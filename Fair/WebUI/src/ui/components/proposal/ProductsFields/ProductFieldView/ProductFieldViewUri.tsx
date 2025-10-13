import { memo } from "react"
import { base64ToUtf8String } from "utils"
import { Link } from "react-router-dom"

function ensureProtocol(uri: string) {
  if (/^(https?:)?\/\//i.test(uri)) return uri;
  return `https://${uri}`;
}

export const ProductFieldViewUri = memo(({ value }: { value: string }) => {
  const rawUri = base64ToUtf8String(value)
  const uri = ensureProtocol(rawUri)

  return (
    <Link to={uri} target="_blank" className="text-blue-600 underline hover:text-blue-800 hover:underline transition-colors duration-150">
      {rawUri}
    </Link>
  )
})
