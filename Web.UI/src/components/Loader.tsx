import { SvgLoadingL, SvgLoadingXl } from "assets"
import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

export type LoaderBaseProps = {
  size?: "large" | "x-large"
}

export type LoaderProps = PropsWithClassName & LoaderBaseProps

export const Loader = memo(({ className, size = "large" }: LoaderProps) => (
  <div className={twMerge("flex h-full w-full items-center justify-center", size, className)}>
    {size === "large" && <SvgLoadingL className="animate-spin fill-dark-blue-100" />}
    {size === "x-large" && <SvgLoadingXl className="animate-spin fill-dark-blue-100" />}
  </div>
))
