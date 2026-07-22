import { useTranslation } from "react-i18next"
import { Link, Navigate, useLocation } from "react-router-dom"

import { useStoreContext } from "app"
import { SvgXSm } from "assets"
import { useGetProductDetails, useGetPublicationDetails } from "entities"
import { useResolveStoreId, useStoreTitle } from "hooks"
import { Breadcrumbs, BreadcrumbsItemProps, ButtonPrimary } from "ui/components"
import { PublicationHeader } from "ui/components/publication"
import { PublicationContentView } from "ui/views"
import { routes } from "utils"

export const PreviewPage = () => {
  const location = useLocation()
  const storeId = useResolveStoreId()
  const { store } = useStoreContext()
  const { t } = useTranslation()

  const productId = location.state?.productId as string | undefined
  const publicationId = location.state?.publicationId as string | undefined
  const previousPath = location.state?.previousPath as string | undefined
  const parentBreadcrumbs = location.state?.parentBreadcrumbs as
    | BreadcrumbsItemProps
    | BreadcrumbsItemProps[]
    | undefined

  const { data: product, isPending: isProductPending } = useGetProductDetails(productId)
  const { data: publication, isPending: isPublicationPending } = useGetPublicationDetails(publicationId)

  const pageTitle = product?.title ?? publication?.title
  useStoreTitle(store?.title, pageTitle ? `Preview - ${pageTitle}` : "Preview")

  // Show logo only for game or software.
  const showLogo =
    (product && (product.type === "game" || product.type === "software")) ||
    (publication && (publication.type === "game" || publication.type === "software"))

  if (!previousPath || (!productId && !publicationId)) return <Navigate to={routes.store(storeId!)} />

  if (isProductPending && isPublicationPending && !product && !publication) {
    return <div>Loading</div>
  }

  return (
    <div className="flex flex-col gap-6">
      {parentBreadcrumbs && (
        <Breadcrumbs
          fullPath={true}
          items={[
            { path: routes.store(storeId!), title: t("common:home") },
            ...(parentBreadcrumbs ? (Array.isArray(parentBreadcrumbs) ? parentBreadcrumbs : [parentBreadcrumbs]) : []),
            { title: product?.id ?? publication?.id ?? "" },
          ]}
        />
      )}
      <PublicationHeader
        title={product?.title ?? publication?.title ?? ""}
        logoFileId={showLogo ? (product?.logoId ?? publication?.logoId) : undefined}
        components={
          previousPath && (
            <Link to={previousPath}>
              <ButtonPrimary
                iconBefore={<SvgXSm className="fill-white" />}
                className="h-11 w-40 capitalize"
                label={t("common:close")}
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
