import Markdown from "react-markdown"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"
import { AboutThreeDotsMenu } from "ui/components/specific"

type AboutBaseInfo = {
  title: string
  description: string
}

export type AboutInfo = PropsWithClassName & AboutBaseInfo

export const AboutInfo = ({ className, title, description }: AboutInfo) => (
  <div className={twMerge("flex flex-col gap-6 rounded-lg bg-gray-100 p-4 sm:p-6", className)}>
    <div className="flex items-center justify-between gap-3">
      <span className="min-w-0 text-2xl leading-8 font-semibold wrap-break-word sm:text-3.5xl sm:leading-10">
        {title}
      </span>
      <AboutThreeDotsMenu />
    </div>
    <Markdown skipHtml={true}>{description}</Markdown>
  </div>
)
