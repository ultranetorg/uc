import { memo, useState } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

type ImageFallbackBaseProps = {
  src?: string
  fallback: JSX.Element
}

export type ImageFallbackProps = PropsWithClassName & ImageFallbackBaseProps

export const ImageFallback = memo(({ className, src, fallback }: ImageFallbackProps) => {
  const [error, setError] = useState(false)

  if (error || !src) {
    return fallback
  }

  return (
    <img
      src={src}
      loading="lazy"
      className={twMerge("size-full object-cover object-center", className)}
      onError={e => {
        e.currentTarget.onerror = null
        setError(true)
      }}
    />
  )
})
