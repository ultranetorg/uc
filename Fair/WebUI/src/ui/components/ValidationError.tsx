import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

export type ValidationErrorBaseProps = {
  message: string
}

export type ValidationErrorProps = PropsWithClassName & ValidationErrorBaseProps

export const ValidationError = memo(({ className, message }: ValidationErrorProps) => (
  <span className={twMerge("text-error text-2xs font-medium leading-4", className)}>{message}</span>
))
