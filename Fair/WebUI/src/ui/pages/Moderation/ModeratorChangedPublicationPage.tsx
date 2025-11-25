import { useCallback } from "react"
import { useTranslation } from "react-i18next"
import { useParams } from "react-router-dom"

import { useGetChangedPublication } from "entities"
import { ModeratorPublicationHeader } from "ui/components/specific"
import { ProductCompareFields } from "ui/components/proposal"

export const ModeratorChangedPublicationPage = () => {
  const { siteId, publicationId } = useParams()
  const { t } = useTranslation()

  const { isFetching, data: publication } = useGetChangedPublication(siteId, publicationId)

  const handleApprove = useCallback(() => alert("approve"), [])
  const handleReject = useCallback(() => alert("reject"), [])
  const handlePreview = useCallback(() => alert("preview"), [])

  if (!siteId || isFetching || !publication) return <div>LOADING</div>

  return (
    <div className="flex flex-col gap-6">
      <ModeratorPublicationHeader
        siteId={siteId!}
        parentBreadcrumb={{ title: t("common:moderation"), path: `/${siteId}/m/n/` }}
        title={publication.publication.title}
        logoFileId={publication.publication.imageId}
        onApprove={handleApprove}
        onPreview={handlePreview}
        onReject={handleReject}
        homeLabel={t("common:home")}
      />
      {/* TODO: ProductCompareFields component should be modified and receive product.versions property instead of productIds to avoid second request to back-end. */}
      <ProductCompareFields publications={[{ id: publication.id, version: publication.latestVersion }]} />
    </div>
  )
}
