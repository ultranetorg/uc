import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { TEST_PUBLISHER_SRC } from "testConfig"
import { buildSrc } from "utils"

export type AuthorImageTitleProps = {
  title: string
  authorAvatar?: string
}

export const AuthorImageTitle = memo(({ title, authorAvatar }: AuthorImageTitleProps) => (
  <div className="flex items-center gap-2">
    <div className="size-8 overflow-hidden rounded-full">
      <img src={buildSrc(authorAvatar, TEST_PUBLISHER_SRC)} className="size-full object-cover" />
    </div>
    <span
      className={twMerge(
        "cursor-pointer overflow-hidden text-ellipsis whitespace-nowrap text-2sm font-medium leading-5 text-gray-800 hover:font-semibold",
      )}
    >
      {title}
    </span>
  </div>
))
