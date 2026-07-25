import { memo, useState } from "react"
import { useTranslation } from "react-i18next"
import { capitalize } from "lodash"

import { useGetAuthor } from "entities"
import { useParams, useResolveStoreId, useStoreTitle } from "hooks"
import { Breadcrumbs } from "ui/components"
import { AuthorProfile } from "ui/components/author"
import { PublisherPublicationsView } from "ui/views"
import { routes } from "utils"

export type PublisherPageProps = {
  showDefaultBreadcrumbs?: boolean
}

export const PublisherPage = memo(({ showDefaultBreadcrumbs = false }: PublisherPageProps) => {
  const { publisherId } = useParams()
  const storeId = useResolveStoreId()
  const { t } = useTranslation()

  const [isModalOpen, setModalOpen] = useState(false)

  const { isPending, data: author, error } = useGetAuthor(publisherId)
  if (error) throw error

  useStoreTitle(author?.title ? `Publisher - ${author?.title}` : undefined)

  if (isPending || !author) {
    return <div>Loading</div>
  }

  return (
    <>
      {showDefaultBreadcrumbs && (
        <Breadcrumbs
          fullPath={true}
          items={[
            { path: routes.store(storeId!), title: t("common:home") },
            { title: capitalize(t("common:publishers")) },
            { title: author?.title },
          ]}
        />
      )}
      <AuthorProfile t={t} size="compact" author={author} showStoreInfo={!showDefaultBreadcrumbs} />
      <PublisherPublicationsView
        size="compact"
        storeId={storeId!}
        author={author}
        isModalOpen={isModalOpen}
        onModalOpenChange={setModalOpen}
      />
    </>
  )
})
