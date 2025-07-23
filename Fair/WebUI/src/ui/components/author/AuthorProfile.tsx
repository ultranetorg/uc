import { memo } from "react"
import { twMerge } from "tailwind-merge"
import { TFunction } from "i18next"

import { TEST_AUTHOR_IMAGE } from "testConfig"
import { PropsWithClassName } from "types"
import { ExpandableText } from "ui/components"
import { buildSrc } from "utils"

import { ProfileLinks } from "./ProfileLinks"

const LABEL_CLASSNAME = "text-2xs font-medium leading-4 text-gray-500"

type AuthorProfileBaseProps = {
  t: TFunction
  title: string
  nickname: string
  avatar?: string
  description: string
  links?: string[]
  registeredDate: number
}

export type AuthorProfileProps = PropsWithClassName & AuthorProfileBaseProps

export const AuthorProfile = memo(
  ({ t, className, title, nickname, avatar, description, links, registeredDate }: AuthorProfileProps) => (
    <div className={twMerge("flex items-start gap-6 rounded-lg border border-gray-300 bg-gray-100 p-6", className)}>
      <div className="h-30 w-30 flex-shrink-0 overflow-hidden rounded-full" title={title}>
        <img src={buildSrc(avatar, TEST_AUTHOR_IMAGE)} className="h-full w-full object-cover" />
      </div>
      <div className="flex flex-col gap-6">
        <div className="flex flex-col gap-1">
          <span className="text-xl font-semibold leading-6">{title}</span>
          <span className="text-2xs leading-4 text-gray-500">{nickname}</span>
        </div>
        {description && (
          <div className="flex flex-col gap-2">
            <span className={LABEL_CLASSNAME}>{t("about")}</span>
            <ExpandableText
              className="text-2sm leading-5"
              text={description}
              readLessLabel={t("readLess")}
              readMoreLabel={t("readMore")}
            />
          </div>
        )}
        {links && links.length > 0 && (
          <div className="flex flex-col gap-2">
            <span className={LABEL_CLASSNAME}>{t("links")}</span>
            <ProfileLinks t={t} links={links} />
          </div>
        )}
        <div className="flex items-center gap-4 text-2sm leading-5">
          <span>{t("author")}</span>
          <span className="leading-3.5 text-gray-500">|</span>
          <span>{registeredDate}</span>
        </div>
      </div>
    </div>
  ),
)
