import { ComponentType, memo } from "react"
import { Link, To } from "react-router-dom"
import { useTranslation } from "react-i18next"

import avatarFallback from "assets/fallback/user-10.png"
import { ImageFallback, LinkFullscreen, RatingBar } from "ui/components"
import { formatDate, routes } from "utils"

const NAME_CLASSNAME = "text-2sm font-semibold leading-4.5"
const TEXT_CLASSNAME = "text-2sm leading-5"
const DATE_CLASSNAME = "text-2xs font-medium leading-4 text-gray-500"

export type CommentStyle = "default" | "compact"

export type CommentContextMenuProps = {
  id: string
  publicationId: string
  reviewerId: string
  reviewerName: string
  text: string
}

type CommentBaseProps = {
  id: string
  creatorId: string
  creatorName: string
  creatorAvatarSrc?: string
  created: number
  rating?: number
  text: string
  publication?: { id: string; title?: string }
  contextMenu?: ComponentType<CommentContextMenuProps>
}

export type CommentProps =
  | (CommentBaseProps & { style?: Extract<CommentStyle, "default">; creatorLink: To })
  | (CommentBaseProps & { style: Extract<CommentStyle, "compact">; creatorLink?: undefined })

export const Comment = memo((props: CommentProps) => {
  const { creatorId, creatorName, creatorAvatarSrc, id, created, rating, text, publication } = props
  const ContextMenu = props.contextMenu
  const { t } = useTranslation()

  const displayName = creatorName ?? creatorId

  return props.style !== "compact" ? (
    <div className="flex flex-col gap-6 rounded-lg border border-gray-300 bg-gray-100 p-6">
      <div className="flex flex-col gap-4">
        <div className="flex gap-4.5">
          <div className="size-13 overflow-hidden rounded-full">
            <ImageFallback src={creatorAvatarSrc} fallbackSrc={avatarFallback} className="size-full object-cover" />
          </div>
          <div className="flex flex-1 flex-col justify-center gap-2">
            <div className="flex items-center justify-between">
              <LinkFullscreen to={props.creatorLink} className={NAME_CLASSNAME} title={displayName}>
                {displayName}
              </LinkFullscreen>
              {ContextMenu && publication && (
                <ContextMenu
                  id={id}
                  reviewerId={creatorId}
                  publicationId={publication.id}
                  reviewerName={displayName}
                  text={text}
                />
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
          <ImageFallback src={creatorAvatarSrc} fallbackSrc={avatarFallback} className="size-full object-cover" />
        </div>
        <div className="flex flex-col gap-1">
          <div className="flex items-center gap-1 leading-4.5">
            <span className={NAME_CLASSNAME}>{displayName}</span>
            {publication && (
              <>
                <span>{t("common:to")}:</span>
                <Link to={routes.publication(publication.id)} className="text-sm font-semibold">
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
})
