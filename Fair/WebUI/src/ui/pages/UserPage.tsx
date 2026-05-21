import { useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { isNumber } from "lodash"

import { useModerationContext } from "app"
import { useGetUserAuthors, useGetUserReviews } from "entities"
import { useUrlParamsState } from "hooks"
import { Breadcrumbs } from "ui/components"
import { UserProfile } from "ui/components/specific"
import { parseInteger } from "utils"

export const UserPage = () => {
  const { isModerator, isPublisher } = useModerationContext()
  const { siteId, userId } = useParams()
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

  if (!user) return <>LOADING</>

  return (
    <div className="flex max-w-182.5 flex-col gap-6">
      <Breadcrumbs
        fullPath={true}
        items={[{ path: `/${siteId}`, title: t("common:home") }, { title: t("common:users") }, { title: user.name }]}
      />
      <UserProfile user={user} reviews={reviews} isPublisher={isPublisher} isModerator={isModerator} />
    </div>
  )
}
