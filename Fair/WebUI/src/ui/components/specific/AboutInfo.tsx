import { PropsWithChildren } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

type AboutBaseInfo = {
  title?: string
}

export type AboutInfo = PropsWithClassName & PropsWithChildren & AboutBaseInfo

export const AboutInfo = ({ className, children, title }: AboutInfo) => (
  <div className={twMerge("flex flex-col gap-6 rounded-lg bg-gray-100 p-6 text-gray-800", className)}>
    {title && <span className="text-3.5xl font-semibold leading-10">{title}</span>}
    <span className="text-2sm leading-5">{children}</span>
  </div>
)
