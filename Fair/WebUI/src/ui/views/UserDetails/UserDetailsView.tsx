import { memo } from "react"
import { useTranslation } from "react-i18next"

import avatarFallback from "assets/fallback/user-30.png"
import { useResolveSiteId } from "hooks"
import { Review, TotalItemsResult, UserAuthors } from "types"
import { CopyAddressButton, ImageFallback } from "ui/components"
import { ModeratorUserMenu } from "ui/components/specific"
import { buildUserAvatarUrl, formatRole } from "utils"

import { ReviewsList } from "./ReviewsList"
import { PublishersList } from "./PublishersList"

const LABEL_CLASSNAME = "text-2base font-medium leading-5 first-letter:uppercase"

export type UserDetailsViewProps = {
  user?: UserAuthors
  reviews?: TotalItemsResult<Review>
  isPublisher: boolean
  isModerator: boolean
}

export const UserDetailsView = memo(({ user, reviews, isPublisher, isModerator }: UserDetailsViewProps) => {
  const siteId = useResolveSiteId()
  const { t } = useTranslation("userDetailsView")

  if (!user || !reviews) return <>LOADING</>

  return (
    <div className="divide-y divide-gray-300 overflow-hidden rounded-lg border border-gray-300 bg-gray-100">
      <div className="flex flex-col">
        <div className="relative h-57.5">
          <div className="relative h-42.5 bg-gray-500">
            <div className="absolute right-6 top-6">
              <ModeratorUserMenu userId={user.id} userName={user.name} />
            </div>
          </div>
          <div className="absolute bottom-0 left-6 size-32 rounded-full bg-white">
            <div className="absolute left-1 top-1 size-30 overflow-hidden rounded-full">
              <ImageFallback
                src={buildUserAvatarUrl(user.id)}
                fallbackSrc={avatarFallback}
                className="size-full object-cover"
              />
            </div>
          </div>
        </div>

        <div className="flex flex-col gap-4 p-6">
          <div className="flex flex-col">
            <span className="text-xl font-semibold leading-6">{user.name}</span>
            <CopyAddressButton address={user.owner} />
          </div>
          <span className="text-2xs capitalize leading-4">{formatRole(t, isPublisher, isModerator)}</span>
        </div>
      </div>
      <div className="flex flex-col gap-5 p-6">
        <span className={LABEL_CLASSNAME}>{t("common:publishers")}</span>
        {user.authors.length > 0 ? (
          <PublishersList siteId={siteId!} authors={user.authors} />
        ) : (
          <>{t("noPublishers")}</>
        )}
      </div>
      <div className="flex flex-col gap-5 p-6">
        <span className={LABEL_CLASSNAME}>{t("reviewsLeft")}</span>
        {reviews.items.length > 0 ? <ReviewsList reviews={reviews} /> : <>{t("noReviews")}</>}
      </div>
    </div>
  )
})
