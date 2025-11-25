import { useCallback } from "react"
import { useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"

import { ProductFields } from "ui/components/proposal"
import { ModeratorPublicationHeader } from "ui/components/specific"
import { useGetUnpublishedProduct } from "entities"

export const ModeratorUnpublishedProductPage = () => {
  const { siteId, productId } = useParams()
  const { t } = useTranslation()

  const { isFetching, data: product } = useGetUnpublishedProduct(siteId, productId)

  const handleApprove = useCallback(() => alert("approve"), [])
  const handleReject = useCallback(() => alert("reject"), [])
  const handlePreview = useCallback(() => alert("preview"), [])

  if (!siteId || isFetching || !product) return <div>LOADING</div>

  return (
    <div className="flex flex-col gap-6">
      <ModeratorPublicationHeader
        siteId={siteId!}
        parentBreadcrumb={{ title: t("common:moderation"), path: `/${siteId}/m/n/` }}
        title={product.title ?? ""}
        logoFileId={product?.logoId}
        onApprove={handleApprove}
        onPreview={handlePreview}
        onReject={handleReject}
        homeLabel={t("common:home")}
      />
      {/* TODO: ProductFields component should be modified and receive product.versions property instead of productIds to avoid second request to back-end. */}
      <ProductFields productIds={[productId!]} />
    </div>
  )
}
