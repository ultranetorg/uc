import { memo, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"

import { useSiteContext } from "app"
import { useGetAuthor } from "entities"
import { useEscapeKey, useParams, useSiteTitle } from "hooks"
import { Breadcrumbs } from "ui/components"
import { AuthorPublicationsView } from "ui/views"
import { routes } from "utils"

export type PublisherPageProps = {
  isFromModeration?: boolean
}

export const PublisherPage = memo(({ isFromModeration = true }: PublisherPageProps) => {
  const { siteId, publisherId } = useParams()
  const { site } = useSiteContext()
  const { t } = useTranslation()

  const [isModalOpen, setModalOpen] = useState(false)

  const { data: author } = useGetAuthor(publisherId)

  useSiteTitle(site?.title, author?.title ? `Publisher -${author?.title}` : "Publisher")

  const breadcrumbs = useMemo(
    () =>
      isFromModeration
        ? [{ path: routes.moderation.publishers(siteId!), title: t("common:publishers") }]
        : [{ title: t("common:members") }],
    [isFromModeration, siteId, t],
  )

  useEscapeKey(() => setModalOpen(false))

  if (!author) {
    return <>LOADING ⏱️</>
  }

  return (
    <div className="flex max-w-[730px] flex-col gap-6">
      <Breadcrumbs
        fullPath={true}
        items={[{ path: routes.site(siteId!), title: t("common:home") }, ...breadcrumbs, { title: author.title }]}
      />
      <AuthorPublicationsView
        size="full"
        siteId={siteId!}
        author={author}
        isModalOpen={isModalOpen}
        onModalOpenChange={setModalOpen}
      />
    </div>
  )
})
