import { memo, useCallback, useState } from "react"
import { useTranslation } from "react-i18next"
import { capitalize, isNumber } from "lodash"

import { useGetUserAuthors, useGetUserReviews } from "entities"
import { useParams, useResolveStoreId, useStoreTitle, useUrlParamsState } from "hooks"
import { Breadcrumbs } from "ui/components"
import { UserDetailsView } from "ui/views"
import { parseInteger, routes } from "utils"
import { REVIEWS_PAGE_SIZE } from "config"

export type ReviewerPageProps = {
  showDefaultBreadcrumbs?: boolean
}

export const ReviewerPage = memo(({ showDefaultBreadcrumbs = false }: ReviewerPageProps) => {
  const { userId } = useParams()
  const storeId = useResolveStoreId()
  const { t } = useTranslation()

  const [state, setState] = useUrlParamsState({
    page: {
      defaultValue: 0,
      parse: v => parseInteger(v),
      validate: v => isNumber(v) && v >= 0,
    },
  })

  const [page, setPage] = useState(state.page)

  const { data: user, error } = useGetUserAuthors(userId)
  if (error) throw error

  const { data: reviews } = useGetUserReviews(user?.id, state.page, REVIEWS_PAGE_SIZE)

  useStoreTitle(user?.name ? `User - ${user?.name}` : undefined)

  const handleReviewsPageChange = useCallback(
    (page: number) => {
      setState({ page: page })
      setPage(page)
    },
    [setState],
  )

  if (!user) return <div>Loading{import.meta.env.DEV ? " (ReviewerPage)" : ""}</div>

  return (
    <div className="flex flex-col gap-6">
      {showDefaultBreadcrumbs && (
        <Breadcrumbs
          fullPath={true}
          items={[
            { path: routes.store(storeId!), title: t("common:home") },
            { title: capitalize(t("common:users")) },
            { title: user.name },
          ]}
        />
      )}
      <UserDetailsView
        storeId={storeId!}
        user={user}
        reviews={reviews}
        reviewsPage={page}
        onReviewsPageChange={handleReviewsPageChange}
      />
    </div>
  )
})
