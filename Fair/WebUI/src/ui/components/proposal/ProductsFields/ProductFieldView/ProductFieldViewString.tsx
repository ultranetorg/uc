import { memo } from "react"
import { base64ToUtf8String } from "utils"

export const ProductFieldViewString = memo(({ value }: { value: string }) => {
  return (<>{base64ToUtf8String(value)}</>)
})
