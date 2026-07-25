import { memo } from "react"
import { useTranslation } from "react-i18next"
import { capitalize, isNumber } from "lodash"

import { useGetUserAuthors, useGetUserReviews } from "entities"
import { useParams, useResolveStoreId, useStoreTitle, useUrlParamsState } from "hooks"
import { Breadcrumbs } from "ui/components"
import { UserDetailsView } from "ui/views"
import { parseInteger, routes } from "utils"

export type ReviewerPageProps = {
  showDefaultBreadcrumbs?: boolean
}

export const ReviewerPage = memo(({ showDefaultBreadcrumbs = false }: ReviewerPageProps) => {
  const { userId } = useParams()
  const storeId = useResolveStoreId()
  const { t } = useTranslation()

  const [state] = useUrlParamsState({
    page: {
      defaultValue: 0,
      parse: v => parseInteger(v),
      validate: v => isNumber(v) && v >= 0,
    },
  })

  const { data: user, error } = useGetUserAuthors(userId)
  if (error) throw error

  const { data: reviews } = useGetUserReviews(user?.id, state.page)

  useStoreTitle(user?.name ? `User - ${user?.name}` : undefined)

  if (!user) return <div>Loading</div>

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
      <UserDetailsView storeId={storeId!} user={user} reviews={reviews} />
    </div>
  )
})
