import { useTranslation } from "react-i18next"
import { Link, Navigate, useLocation, useParams } from "react-router-dom"

import { SvgXSm } from "assets"
import { useGetProductDetails } from "entities/Product"
import { PageType } from "types"
import { ButtonPrimary } from "ui/components"
import { PublicationContentView } from "ui/views"
import { SoftwarePublicationHeader } from "ui/components/publication"
import { useGetPublication } from "entities"

export const PreviewPage = () => {
  const location = useLocation()
  const { siteId } = useParams()
  const { t } = useTranslation("previewPage")

  const productId = location.state?.productId as string | undefined
  const proposalId = location.state?.proposalId as string | undefined
  const publicationId = location.state?.publicationId as string | undefined
  const navigatedFromSource = location.state?.source as PageType | undefined

  const { data: product, isPending: isProductPending } = useGetProductDetails(productId)
  const { data: publication, isPending: isPublicationPending } = useGetPublication(publicationId)

  if (!navigatedFromSource || (!productId && !publicationId)) return <Navigate to={`/${siteId}`} />

  if (isProductPending && isPublicationPending && !product && !publication) {
    return <>🕑 LOADING</>
  }

  return (
    <div className="flex flex-col gap-6">
      <SoftwarePublicationHeader
        title={product?.title ?? publication?.title ?? ""}
        logoFileId={product?.logoFileId ?? publication?.logoFileId}
        components={
          navigatedFromSource && (
            <Link
              to={
                navigatedFromSource === "ModeratorCreatePublicationPage"
                  ? `/${siteId}/m/new-publication?productId=${productId}`
                  : `/${siteId}/m/c/p/${proposalId}`
              }
            >
              <ButtonPrimary
                iconBefore={<SvgXSm className="fill-white" />}
                className="h-11 w-61"
                label={t("closePreview")}
              />
            </Link>
          )
        }
      />
      <div className="flex gap-8">
        <PublicationContentView
          isPending={isProductPending || isPublicationPending}
          productOrPublication={product! ?? publication!}
        />
      </div>
    </div>
  )
}
