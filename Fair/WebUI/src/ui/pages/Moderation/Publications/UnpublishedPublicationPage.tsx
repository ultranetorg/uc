import { useMemo } from "react"
import { Link } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { truncate } from "lodash"

import { useOperationPolicy, useSiteContext } from "app"
import { SvgEyeSm } from "assets"
import { unpublishedPublicationsKeys, useGetUnpublishedPublication } from "entities"
import { useParams, useResolveSiteId, useSiteTitle } from "hooks"
import { OperationClass } from "types"
import { ModerationHeader, ModerationPublicationHeader, ProductFieldsTree } from "ui/components/specific"
import { ButtonBar, ButtonOutline, ButtonPrimary } from "ui/components"
import { routes } from "utils"

export const UnpublishedPublicationPage = () => {
  const { publicationId } = useParams()
  const siteId = useResolveSiteId()
  const { t } = useTranslation("unpublishedPublicationPage")

  const { voterId } = useOperationPolicy("publication-updation")
  const { site } = useSiteContext()

  const parentBreadcrumbs = useMemo(
    () => [
      { title: t("common:publications"), path: routes.moderation.publications(siteId!) },
      { title: t("common:unpublished"), path: routes.moderation.publications(siteId!, "u") },
    ],
    [siteId, t],
  )

  const { isLoading, data: publication } = useGetUnpublishedPublication(siteId, publicationId)

  const pageTitle = publication?.title ?? publication?.id
  useSiteTitle(site?.title, pageTitle ? `Unpublished Publication - ${pageTitle}` : "Unpublished Publication")

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
                  to={routes.moderation.create(siteId!)}
                  state={{
                    parentBreadcrumbs,
                    title: publication.title
                      ? `Publish "${truncate(publication.title, { length: 46 })}" product`
                      : t("publishNoTitle"),
                    type: "publication-publish" as OperationClass,
                    publicationId: publication.id,
                    redirectAfterProposalCreation: routes.moderation.publications(siteId!, "p"),
                    redirectAfterProposalExecution: routes.moderation.publications(siteId!, "u"),
                    invalidateQueryKeys: unpublishedPublicationsKeys.all(siteId!),
                  }}
                >
                  <ButtonPrimary className="h-11 w-40 capitalize" label={t("common:publish")} />
                </Link>
                <Link
                  to={routes.moderation.preview(siteId!)}
                  state={{
                    publicationId: publication.id,
                    previousPath: routes.moderation.unpublishedPublication(siteId!, publication.id),
                    parentBreadcrumbs,
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

      <div className="flex flex-col gap-6 rounded-lg bg-gray-100 p-6">
        <ModerationPublicationHeader
          title={publication.title}
          logoId={publication.logoId}
          authorId={publication.authorId}
          authorTitle={publication.authorTitle}
        />
        <ProductFieldsTree productFields={publication.fields} />
      </div>
    </div>
  )
}
