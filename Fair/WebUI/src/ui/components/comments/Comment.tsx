import { ComponentType, memo } from "react"

import { Link } from "react-router-dom"
import { useTranslation } from "react-i18next"
import avatarFallback from "assets/fallback/user-10.png"
import { useResolveSiteId } from "hooks"
import { AccountBaseAvatar } from "types"
import { ImageFallback, RatingBar } from "ui/components"
import { buildUserAvatarUrl, formatDate, routes } from "utils"

const NAME_CLASSNAME = "text-2sm font-semibold leading-4.5"
const TEXT_CLASSNAME = "text-2sm leading-5"
const DATE_CLASSNAME = "text-2xs font-medium leading-4 text-gray-500"

export type CommentStyle = "default" | "compact"

export type CommentContextMenuProps = {
  id: string
  reviewerId: string
  reviewerName: string
  text: string
}

export type CommentProps = {
  style?: CommentStyle
  account: AccountBaseAvatar
  id: string
  created: number
  rating?: number
  text: string
  publication?: { id: string; title?: string }
  contextMenu?: ComponentType<CommentContextMenuProps>
}

export const Comment = memo(
  ({ style = "default", account, id, created, rating, text, publication, contextMenu: ContextMenu }: CommentProps) => {
    const siteId = useResolveSiteId()
    const { t } = useTranslation()

    const displayName = account.nickname ?? account.id

    return style === "default" ? (
      <div className="flex flex-col gap-6 rounded-lg border border-gray-300 bg-gray-100 p-6">
        <div className="flex flex-col gap-4">
          <div className="flex gap-4.5">
            <div className="size-13 overflow-hidden rounded-full">
              <ImageFallback
                src={buildUserAvatarUrl(account.id)}
                fallbackSrc={avatarFallback}
                className="size-full object-cover"
              />
            </div>
            <div className="flex flex-1 flex-col justify-center gap-2">
              <div className="flex items-center justify-between">
                <Link to={routes.user(siteId!, account.id)} className={NAME_CLASSNAME} title={displayName}>
                  {displayName}
                </Link>
                {ContextMenu && (
                  <ContextMenu id={id} reviewerId={account.id} reviewerName={account.nickname} text={text} />
                )}
              </div>
              <span className={DATE_CLASSNAME}>{formatDate(created)}</span>
            </div>
          </div>
          {rating && <RatingBar value={rating} />}
        </div>
        <span className={TEXT_CLASSNAME}>{text}</span>
      </div>
    ) : (
      <div className="flex flex-col gap-4 rounded-lg border border-gray-300 bg-white p-4">
        <div className="flex items-center gap-2">
          <div className="size-10 overflow-hidden rounded-full">
            <ImageFallback
              src={buildUserAvatarUrl(account.id)}
              fallbackSrc={avatarFallback}
              className="size-full object-cover"
            />
          </div>
          <div className="flex flex-col gap-1">
            <div className="flex items-center gap-1 leading-4.5">
              <span className={NAME_CLASSNAME}>{displayName}</span>
              {publication && (
                <>
                  <span>{t("common:to")}:</span>
                  <Link to={routes.publication(siteId!, publication.id)} className="text-sm font-semibold">
                    {publication.title}
                  </Link>
                </>
              )}
            </div>
            <span className={DATE_CLASSNAME}>{formatDate(created)}</span>
          </div>
        </div>
        {rating && <RatingBar value={rating} />}
        <span className={TEXT_CLASSNAME}>{text}</span>
      </div>
    )
  },
)
