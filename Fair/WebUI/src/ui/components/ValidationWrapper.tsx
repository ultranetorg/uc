import { memo, PropsWithChildren } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

export type ValidationWrapperBaseProps = {
  message?: string
}

export type ValidationWrapperProps = PropsWithChildren & PropsWithClassName & ValidationWrapperBaseProps

export const ValidationWrapper = memo(({ children, className, message }: ValidationWrapperProps) =>
  !message ? (
    children
  ) : (
    <div className={twMerge("flex flex-col gap-1", className)}>
      {children}
      <span className={twMerge("text-2xs font-medium leading-4 text-error", className)}>{message}</span>
    </div>
  ),
)
