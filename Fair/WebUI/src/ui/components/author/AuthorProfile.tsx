import { memo } from "react"
import { twMerge } from "tailwind-merge"
import { TFunction } from "i18next"
import { Link } from "react-router-dom"

import { useStoreContext } from "app"
import avatarFallback from "assets/fallback/author-30.png"
import { AuthorDetails, PropsWithClassName } from "types"
import { ExpandableText, ImageFallback } from "ui/components"
import { buildFileUrl, formatRole, isAuthorModerator, isAuthorPublisher, routes } from "utils"

import { ProfileLinks } from "./ProfileLinks"

const LABEL_CLASSNAME = "text-2xs font-medium leading-4 text-gray-500 capitalize"

type AuthorProfileBaseProps = {
  t: TFunction
  size?: "compact" | "full"
  author: AuthorDetails
  showStoreInfo?: boolean
}

export type AuthorProfileProps = PropsWithClassName & AuthorProfileBaseProps

export const AuthorProfile = memo(({ t, className, size = "compact", author, showStoreInfo }: AuthorProfileProps) => {
  const { store: site } = useStoreContext()

  const isPublisher = isAuthorPublisher(site, author)
  const isModerator = isAuthorModerator(site, author)

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
      <span className="text-xl font-semibold leading-6">
        {author.title}{" "}
        {site && showStoreInfo && (
          <>
            on{" "}
            <Link to={routes.store(site.id)} className="underline">
              {site.title} store
            </Link>
          </>
        )}
      </span>
      <span className="flex gap-2 text-2xs leading-4 text-gray-500">
        {author.ownersIds.map(x => (
          <span key={x.id}>{x.name}</span>
        ))}
      </span>
    </div>
  )

  const renderAbout = () => (
    <div className="flex flex-col gap-2">
      <span className={LABEL_CLASSNAME}>{t("authorProfile:about")}</span>
      <ExpandableText
        className="text-2sm leading-5"
        text={author.description}
        readLessLabel={t("authorProfile:readLess")}
        readMoreLabel={t("authorProfile:readMore")}
      />
    </div>
  )

  const renderLinks = () => (
    <div className="flex flex-col gap-2">
      <span className={LABEL_CLASSNAME}>{t("authorProfile:links")}</span>
      <ProfileLinks t={t} links={author.references!} authorLink={site != null ? routes.author(author.id) : undefined} />
    </div>
  )

  const renderAuthorInfo = () =>
    site ? (
      <div className="flex flex-col gap-2">
        <span className={LABEL_CLASSNAME}>{t("authorProfile:roles")}</span>
        <span className="capitalize">{formatRole(t, isPublisher, isModerator)}</span>
      </div>
    ) : null

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
})
