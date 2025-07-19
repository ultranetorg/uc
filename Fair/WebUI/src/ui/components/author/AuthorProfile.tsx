import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { TEST_AUTHOR_IMAGE } from "testConfig"
import { AuthorLink, PropsWithClassName } from "types"
import { ExpandableText } from "ui/components"
import { buildSrc } from "utils"

import { ProfileLinks } from "./ProfileLinks"

const LABEL_CLASSNAME = "text-2xs font-medium leading-4 text-gray-500"

type AuthorProfileBaseProps = {
  title: string
  nickname: string
  avatar?: string
  description: string
  links?: AuthorLink[]
  registeredDate: number
  aboutLabel: string
  authorLabel: string
  linksLabel: string
  readLessLabel: string
  readMoreLabel: string
}

export type AuthorProfileProps = PropsWithClassName & AuthorProfileBaseProps

export const AuthorProfile = memo(
  ({
    className,
    title,
    nickname,
    avatar,
    description,
    links,
    registeredDate,
    aboutLabel,
    authorLabel,
    linksLabel,
    readLessLabel,
    readMoreLabel,
  }: AuthorProfileProps) => (
    <div className={twMerge("flex items-start gap-6 rounded-lg border border-gray-300 bg-gray-100 p-6", className)}>
      <div className="h-30 w-30 flex-shrink-0 overflow-hidden rounded-full" title={title}>
        <img src={buildSrc(avatar, TEST_AUTHOR_IMAGE)} className="h-full w-full object-cover" />
      </div>
      <div className="flex flex-col gap-6">
        <div className="flex flex-col gap-1">
          <span className="text-xl font-semibold leading-6">{title}</span>
          <span className="text-2xs leading-4 text-gray-500">{nickname}</span>
        </div>
        <div className="flex flex-col gap-2">
          <span className={LABEL_CLASSNAME}>{aboutLabel}</span>
          <ExpandableText
            className="text-2sm leading-5"
            text={description}
            readLessLabel={readLessLabel}
            readMoreLabel={readMoreLabel}
          />
        </div>
        {links && links.length > 0 && (
          <div className="flex flex-col gap-2">
            <span className={LABEL_CLASSNAME}>{linksLabel}</span>
            <ProfileLinks links={links} />
          </div>
        )}
        <div className="flex items-center gap-4 text-2sm leading-5">
          <span>{authorLabel}</span>
          <span className="leading-3.5 text-gray-500">|</span>
          <span>{registeredDate}</span>
        </div>
      </div>
    </div>
  ),
)
