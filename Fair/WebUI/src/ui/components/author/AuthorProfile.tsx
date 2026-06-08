import { memo } from "react"
import { twMerge } from "tailwind-merge"
import { TFunction } from "i18next"

import avatarFallback from "assets/fallback/account-avatar-30.png"
import { AuthorDetails, PropsWithClassName } from "types"
import { ExpandableText, ImageFallback } from "ui/components"
import { buildFileUrl, formatRole } from "utils"

import { ProfileLinks } from "./ProfileLinks"

const LABEL_CLASSNAME = "text-2xs font-medium leading-4 text-gray-500"

type AuthorProfileBaseProps = {
  t: TFunction
  size?: "compact" | "full"
  author: AuthorDetails
  isPublisher: boolean
  isModerator: boolean
}

export type AuthorProfileProps = PropsWithClassName & AuthorProfileBaseProps

export const AuthorProfile = memo(
  ({ t, className, size = "compact", author, isPublisher, isModerator }: AuthorProfileProps) => {
    const renderAvatar = () => (
      <div
        className={twMerge("size-30 shrink-0 overflow-hidden rounded-full", size === "full" && "size-20")}
        title={author.title}
      >
        <ImageFallback
          src={buildFileUrl(author.avatarId)}
          fallbackSrc={avatarFallback}
          className={twMerge("size-full object-cover", size === "full" && "size-20")}
        />
      </div>
    )

    const renderTitle = () => (
      <div className="flex flex-col gap-1">
        <span className="text-xl font-semibold leading-6">{author.title}</span>
        <span className="flex gap-2 text-2xs leading-4 text-gray-500">
          {author.ownersIds.map(x => (
            <span key={x.id}>{x.name}</span>
          ))}
        </span>
      </div>
    )

    const renderAbout = () => (
      <div className="flex flex-col gap-2">
        <span className={LABEL_CLASSNAME}>{t("about")}</span>
        <ExpandableText
          className="text-2sm leading-5"
          text={author.description}
          readLessLabel={t("readLess")}
          readMoreLabel={t("readMore")}
        />
      </div>
    )

    const renderLinks = () => (
      <div className="flex flex-col gap-2">
        <span className={LABEL_CLASSNAME}>{t("links")}</span>
        <ProfileLinks t={t} links={author.references!} />
      </div>
    )

    const renderAuthorInfo = () => (
      <div className="flex items-center gap-4 text-2sm capitalize leading-5">
        <span>{formatRole(t, isPublisher, isModerator)}</span>
      </div>
    )

    return (
      <div className={twMerge("flex items-start gap-6 rounded-lg border border-gray-300 bg-gray-100 p-6", className)}>
        {size === "compact" ? (
          <>
            {renderAvatar()}
            <div className="flex flex-col gap-6">
              {renderTitle()}
              {author.description && renderAbout()}
              {author.references && author.references.length > 0 && renderLinks()}
              {renderAuthorInfo()}
            </div>
          </>
        ) : (
          <div className="flex flex-col gap-6">
            <div className="flex items-center gap-4">
              {renderAvatar()}
              {renderTitle()}
            </div>
            {author.description && renderAbout()}
            {author.references && author.references.length > 0 && renderLinks()}
            {renderAuthorInfo()}
          </div>
        )}
      </div>
    )
  },
)
