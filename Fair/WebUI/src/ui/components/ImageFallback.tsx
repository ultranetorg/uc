import { memo, useState } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

type WithFallbackElement = {
  src?: string
  fallback: JSX.Element
  fallbackSrc?: never
}

type WithFallbackSrc = {
  src?: string
  fallbackSrc: string
  fallback?: never
}

type ImageFallbackBaseProps = WithFallbackElement | WithFallbackSrc

export type ImageFallbackProps = PropsWithClassName & ImageFallbackBaseProps

export const ImageFallback = memo(({ className, src, fallbackSrc, fallback }: ImageFallbackProps) => {
  const [showFallbackElement, setShowFallbackElement] = useState(false)

  if (showFallbackElement || !src) {
    return fallback ?? null
  }

  return (
    <img
      src={src}
      loading="lazy"
      className={twMerge("size-full object-cover object-center", className)}
      onError={e => {
        e.currentTarget.onerror = null

        if (fallbackSrc) {
          e.currentTarget.src = fallbackSrc
        } else {
          setShowFallbackElement(true)
        }
      }}
    />
  )
})
