import { memo } from "react"
import { base64ToUtf8String } from "utils"
import { Link } from "react-router-dom"


export const ProductFieldViewVideo = memo(({ value }: { value: string }) => {
  const rawUri = base64ToUtf8String(value)

  return (
    <Link to={rawUri} target="_blank" className="text-blue-600 underline hover:text-blue-800 hover:underline transition-colors duration-150">
      {rawUri}
    </Link>
  )
})
