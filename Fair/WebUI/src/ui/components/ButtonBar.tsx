import { PropsWithChildren } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

type ButtonBarOrientation = "horizontal" | "vertical"

export type ButtonBarBaseProps = {
  orientation?: ButtonBarOrientation
}

export type ButtonBarProps = PropsWithChildren & PropsWithClassName & ButtonBarBaseProps

export const ButtonBar = ({ children, className, orientation = "horizontal" }: ButtonBarProps) => (
  <div className={twMerge("flex gap-4", orientation === "vertical" && "flex-col", className)}>{children}</div>
)
