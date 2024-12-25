import { ReactNode, memo } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

type ContainerProps = {
  children: ReactNode
} & PropsWithClassName

export const Container = memo(({ className, children }: ContainerProps) => {
  return <div className={twMerge("mx-auto w-full max-w-6xl px-4", className)}>{children}</div>
})
