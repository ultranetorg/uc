import { memo, useState } from "react"
import { useTranslation } from "react-i18next"

import { useGetAuthor } from "entities"
import { useParams, useResolveSiteId, useSiteTitle } from "hooks"
import { Breadcrumbs } from "ui/components"
import { AuthorPublicationsView } from "ui/views"
import { routes } from "utils"

export type PublisherPageProps = {
  showDefaultBreadcrumbs?: boolean
}

export const PublisherPage = memo(({ showDefaultBreadcrumbs = false }: PublisherPageProps) => {
  const { publisherId } = useParams()
  const siteId = useResolveSiteId()
  const { t } = useTranslation()

  const [isModalOpen, setModalOpen] = useState(false)

  const { isPending, data: author } = useGetAuthor(publisherId)

  useSiteTitle(author?.title ? `Publisher - ${author?.title}` : undefined)

  if (isPending || !author) {
    return <div>Loading PublisherPage</div>
  }

  return (
    <>
      {showDefaultBreadcrumbs && (
        <Breadcrumbs
          fullPath={true}
          items={[
            { path: routes.site(siteId!), title: t("common:home") },
            { title: t("common:publishers") },
            { title: author?.title },
          ]}
        />
      )}
      <AuthorPublicationsView
        size="compact"
        siteId={siteId!}
        author={author}
        isModalOpen={isModalOpen}
        onModalOpenChange={setModalOpen}
      />
    </>
  )
})
