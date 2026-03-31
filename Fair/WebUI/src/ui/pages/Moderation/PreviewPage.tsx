import { useTranslation } from "react-i18next"
import { Link, useParams } from "react-router-dom"

import { SvgXSm } from "assets"
import { useGetProductDetails } from "entities/Product"
import { ButtonPrimary } from "ui/components"
import { PublicationContentView } from "ui/views"
import { SoftwarePublicationHeader } from "ui/components/publication"

export const PreviewPage = () => {
  const { siteId, productId } = useParams()
  const { t } = useTranslation("previewPage")

  const { data: product, isFetching } = useGetProductDetails(productId)

  if (!product) {
    return <>🕑 LOADING</>
  }

  return (
    <div className="flex flex-col gap-6">
      <SoftwarePublicationHeader
        title={product.title ?? ""}
        logoFileId={product.logoFileId}
        components={
          <Link to={`/${siteId}/m/new-publication?productId=${productId}`}>
            <ButtonPrimary
              iconBefore={<SvgXSm className="fill-white" />}
              className="h-11 w-61"
              label={t("closePreview")}
            />
          </Link>
        }
      />
      <div className="flex gap-8">
        <PublicationContentView isPending={isFetching} productOrPublication={product} />
      </div>
    </div>
  )
}
