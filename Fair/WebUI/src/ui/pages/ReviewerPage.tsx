import { memo } from "react"
import { useTranslation } from "react-i18next"
import { isNumber } from "lodash"

import { useSiteRolesContext } from "app"
import { useGetUserAuthors, useGetUserReviews } from "entities"
import { useParams, useResolveSiteId, useSiteTitle, useUrlParamsState } from "hooks"
import { Breadcrumbs } from "ui/components"
import { UserDetailsView } from "ui/views"
import { parseInteger, routes } from "utils"

export type ReviewerPageProps = {
  isFromModeration?: boolean
}

export const ReviewerPage = memo(({ isFromModeration = true }: ReviewerPageProps) => {
  const { isModerator, isPublisher } = useSiteRolesContext()
  const { reviewerId } = useParams()
  const siteId = useResolveSiteId()
  const { t } = useTranslation()

  const [state] = useUrlParamsState({
    page: {
      defaultValue: 0,
      parse: v => parseInteger(v),
      validate: v => isNumber(v) && v >= 0,
    },
  })

  const { data: user } = useGetUserAuthors(reviewerId)
  const { data: reviews } = useGetUserReviews(user?.id, state.page)

  useSiteTitle(user?.name ? `User - ${user?.name}` : undefined)

  if (!user) return <>LOADING ReviewerPage</>

  return (
    <div className="flex max-w-182.5 flex-col gap-6">
      {!isFromModeration && (
        <Breadcrumbs
          fullPath={true}
          items={[
            { path: routes.site(siteId!), title: t("common:home") },
            { title: t("common:users") },
            { title: user.name },
          ]}
        />
      )}
      <UserDetailsView
        siteId={siteId!}
        user={user}
        reviews={reviews}
        isPublisher={isPublisher}
        isModerator={isModerator}
      />
    </div>
  )
})
