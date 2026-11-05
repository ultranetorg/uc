import { memo } from "react"
import { useTranslation } from "react-i18next"

import { useStoreContext } from "app"
import avatarFallback from "assets/fallback/user-avatar-6xl.png"
import { REVIEWS_PAGE_SIZE } from "config"
import { useGetUserStoreExists } from "entities"
import { Review, TotalItemsResult, UserAuthors } from "types"
import { CopyAddressButton, ImageFallback, Pagination, Spinner } from "ui/components"
import { UserDetailsThreeDotsMenu } from "ui/components/specific"
import { renderRole } from "ui/renderers"
import { buildUserAvatarByIdUrl, isUserModerator, isUserPublisher } from "utils"

import { ReviewsList } from "./ReviewsList"
import { PublishersList } from "./PublishersList"

const LABEL_CLASSNAME = "text-2base font-medium leading-5 first-letter:uppercase"

export type UserDetailsViewProps = {
  storeId: string
  user?: UserAuthors
  reviews?: TotalItemsResult<Review>
  isReviewsPending: boolean
  reviewsPage: number
  onReviewsPageChange: (page: number) => void
}

export const UserDetailsView = memo(
  ({ storeId, user, reviews, isReviewsPending, reviewsPage, onReviewsPageChange }: UserDetailsViewProps) => {
    const { t } = useTranslation("userDetailsView")

    const { store } = useStoreContext()

    const { data: isJoined } = useGetUserStoreExists(user?.id, store?.id)

    if (!user) return <div>Loading{import.meta.env.DEV ? " (UserDetailsView)" : ""}</div>

    const isPublisher = isUserPublisher(
      store,
      user.authors?.map(x => x.id),
    )
    const isModerator = isUserModerator(store, user.id)
    const pagesCount =
      reviews?.totalItems && reviews.totalItems > 0 ? Math.ceil(reviews.totalItems / REVIEWS_PAGE_SIZE) : 0

    return (
      <div className="divide-y divide-gray-300 overflow-hidden rounded-lg border border-gray-300 bg-gray-100">
        <div className="flex flex-col">
          <div className="relative h-57.5">
            <div className="relative h-42.5 bg-gray-500">
              <div className="absolute right-6 top-6">
                <UserDetailsThreeDotsMenu userId={user.id} userName={user.name} />
              </div>
            </div>
            <div className="absolute bottom-0 left-6 size-32 rounded-full bg-white">
              <div className="absolute left-1 top-1 size-30 overflow-hidden rounded-full">
                <ImageFallback
                  src={buildUserAvatarByIdUrl(user.id)}
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
            <div className="text-2xs leading-4">{renderRole(t, isPublisher, isModerator, isJoined)}</div>
          </div>
        </div>
        <div className="flex flex-col gap-5 p-6">
          <span className={LABEL_CLASSNAME}>{t("common:publishers")}</span>
          {user.authors.length > 0 ? (
            <PublishersList storeId={storeId} authors={user.authors} />
          ) : (
            <>{t("noPublishers")}</>
          )}
        </div>
        <div className="flex flex-col gap-5 p-6">
          <span className={LABEL_CLASSNAME}>{t("reviewsLeft")}</span>
          {isReviewsPending || !reviews ? (
            <Spinner />
          ) : reviews.items.length > 0 ? (
            <ReviewsList reviews={reviews} />
          ) : (
            <>{t("noReviews")}</>
          )}
          <Pagination
            className="self-end"
            pagesCount={pagesCount}
            onPageChange={onReviewsPageChange}
            page={reviewsPage}
          />
        </div>
      </div>
    )
  },
)
