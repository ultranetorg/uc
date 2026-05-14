import { useState } from "react"
import { useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"

import { useGetAuthor } from "entities"
import { Breadcrumbs } from "ui/components"
import { AuthorPublicationsView } from "ui/views"
import { useEscapeKey } from "hooks"

export const PublisherPage = () => {
  const { siteId, publisherId } = useParams()
  const { t } = useTranslation()

  const [isModalOpen, setModalOpen] = useState(false)

  const { data: author } = useGetAuthor(publisherId)

  useEscapeKey(() => setModalOpen(false))

  if (!author) {
    return <>LOADING ⏱️</>
  }

  return (
    <div className="flex max-w-[730px] flex-col gap-6">
      <Breadcrumbs
        fullPath={true}
        items={[
          { path: `/${siteId}`, title: t("common:home") },
          { path: `/${siteId}/m/`, title: t("common:proposals") },
          { path: `/${siteId}/m/a/`, title: t("common:publishers") },
          { title: author.title },
        ]}
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
}
