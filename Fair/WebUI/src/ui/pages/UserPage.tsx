import { memo } from "react"
import { useTranslation } from "react-i18next"
import { isNumber } from "lodash"

import { useGetUserAuthors, useGetUserReviews } from "entities"
import { useParams, useResolveSiteId, useSiteTitle, useUrlParamsState } from "hooks"
import { Breadcrumbs } from "ui/components"
import { UserDetailsView } from "ui/views"
import { parseInteger, routes } from "utils"

export type UserPageProps = {
  isFromModeration?: boolean
}

export const UserPage = memo(({ isFromModeration = true }: UserPageProps) => {
  const { userId } = useParams()
  const siteId = useResolveSiteId()
  const { t } = useTranslation()

  const [state] = useUrlParamsState({
    page: {
      defaultValue: 0,
      parse: v => parseInteger(v),
      validate: v => isNumber(v) && v >= 0,
    },
  })

  const { data: user } = useGetUserAuthors(userId)
  const { data: reviews } = useGetUserReviews(user?.id, state.page)

  useSiteTitle(user?.name ? `User - ${user?.name}` : undefined)

  if (!user) return <div>Loading</div>

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
      <UserDetailsView siteId={siteId!} user={user} reviews={reviews} />
    </div>
  )
})
