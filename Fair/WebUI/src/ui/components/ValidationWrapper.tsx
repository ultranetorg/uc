import { memo, PropsWithChildren } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

export type ValidationWrapperType = "default" | "error" | "success"

export type ValidationWrapperBaseProps = {
  message?: string
  type?: ValidationWrapperType
}

export type ValidationWrapperProps = PropsWithChildren & PropsWithClassName & ValidationWrapperBaseProps

export const ValidationWrapper = memo(({ children, className, message, type = "error" }: ValidationWrapperProps) => (
  <div className={className}>
    {children}
    {message && (
      <span
        className={twMerge(
          "mt-1 text-2xs font-medium leading-4",
          type === "default" && "text-gray-500",
          type === "error" && "text-error",
          type === "success" && "text-light-green",
          className,
        )}
      >
        {message}
      </span>
    )}
  </div>
))
