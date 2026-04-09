import { Link, useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"

import { useMemo } from "react"
import { useModerationContext } from "app"
import { SvgEyeSm } from "assets"
import { useGetUnpublishedPublication } from "entities"
import { OperationClass } from "types"
import { ModerationHeader, ModerationPublicationHeader, ProductFieldsTree } from "ui/components/specific"
import { ButtonBar, ButtonOutline, ButtonPrimary } from "ui/components"

export const UnpublishedPublicationPage = () => {
  const { siteId, publicationId } = useParams()
  const { getOperationVoterId } = useModerationContext()
  const { t } = useTranslation("unpublishedPublicationPage")

  const voterId = getOperationVoterId("publication-updation")

  const parentBreadcrumbs = useMemo(
    () => [
      { title: t("common:proposals"), path: `/${siteId}/m` },
      { title: t("common:publications"), path: `/${siteId}/m/c` },
      { title: t("common:unpublished"), path: `/${siteId}/m/c/u` },
    ],
    [siteId, t],
  )

  const { isLoading, data: publication } = useGetUnpublishedPublication(siteId, publicationId)

  if (isLoading || !publication) return <div>LOADING</div>

  return (
    <div className="flex flex-col gap-6">
      <ModerationHeader
        title={publication.id}
        parentBreadcrumbs={parentBreadcrumbs}
        components={
          <>
            {!!voterId && (
              <ButtonBar className="items-center">
                <Link
                  to={`/${siteId}/m/new`}
                  state={{
                    parentBreadcrumbs,
                    title: publication.title
                      ? t("publish", { publicationTitle: publication.title })
                      : t("publishNoTitle"),
                    type: "publication-publish" as OperationClass,
                    publicationId: publication.id,
                    previousPath: `/${siteId}/m/c`,
                  }}
                >
                  <ButtonPrimary className="h-11 w-40 capitalize" label={t("common:publish")} />
                </Link>
                <Link
                  to={`/${siteId}/m/v`}
                  state={{
                    publicationId: publication.id,
                    previousPath: `/${siteId}/m/c/u/${publication.id}`,
                  }}
                >
                  <ButtonOutline
                    className="h-11 w-40 capitalize"
                    label={t("common:preview")}
                    iconBefore={<SvgEyeSm className="fill-gray-800" />}
                  />
                </Link>
              </ButtonBar>
            )}
          </>
        }
      />

      <ModerationPublicationHeader
        title={publication.title}
        logoId={publication.logoId}
        authorId={publication.authorId}
        authorTitle={publication.authorTitle}
      />
      <ProductFieldsTree productFields={publication.fields} />
    </div>
  )
}
