import { memo } from "react"
import { base64ToUtf8String } from "utils"

export const ProductFieldViewDate = memo(({ value }: { value: string }) => {
  const date = new Date(base64ToUtf8String(value))

  const formattedDate = new Intl.DateTimeFormat("en-US", {
    year: "numeric",
    month: "long",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit",
    timeZoneName: "short",
  }).format(date)

  return <>{formattedDate}</>
})
