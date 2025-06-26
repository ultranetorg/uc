import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

type AboutBaseInfo = {
  title: string
  description: string
}

export type AboutInfo = PropsWithClassName & AboutBaseInfo

export const AboutInfo = ({ className, title, description }: AboutInfo) => (
  <div className={twMerge("flex flex-col gap-6 rounded-lg bg-gray-100 p-6 text-gray-800", className)}>
    <span className="text-3.5xl font-semibold leading-10">{title}</span>
    <span className="text-2sm leading-5">{description}</span>
  </div>
)
