import { memo } from "react"
import { base64ToBigInt } from "utils"

export const ProductFieldViewBigInt = memo(({ value }: { value: string }) => {
  return (<>{String(base64ToBigInt(value))}</>)
})
