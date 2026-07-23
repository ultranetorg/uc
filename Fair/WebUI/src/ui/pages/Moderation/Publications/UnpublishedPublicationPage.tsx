import { useMemo } from "react"
import { Link } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { truncate } from "lodash"

import { useOperationPolicy, useStoreContext } from "app"
import { SvgEyeSm } from "assets"
import { unpublishedPublicationsKeys, useGetUnpublishedPublication } from "entities"
import { useParams, useResolveStoreId, useStoreTitle } from "hooks"
import { OperationType } from "types"
import { ModerationHeader, ModerationPublicationHeader, ProductFieldsTree } from "ui/components/specific"
import { ButtonBar, ButtonOutline, ButtonPrimary } from "ui/components"
import { routes } from "utils"

export const UnpublishedPublicationPage = () => {
  const { publicationId } = useParams()
  const storeId = useResolveStoreId()
  const { t } = useTranslation("unpublishedPublicationPage")

  const { voterId } = useOperationPolicy("publication-updation")
  const { store } = useStoreContext()

  const parentBreadcrumbs = useMemo(
    () => [{ title: t("common:publications"), path: routes.moderation.publications(storeId!, "unpublished") }],
    [storeId, t],
  )

  const { isLoading, data: publication } = useGetUnpublishedPublication(storeId, publicationId)

  const pageTitle = publication?.title ?? publication?.id
  useStoreTitle(store?.title, pageTitle ? `Unpublished Publication - ${pageTitle}` : "Unpublished Publication")

  if (isLoading || !publication) return <div>Loading</div>

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
                  to={routes.moderation.createProposal(storeId!)}
                  state={{
                    parentBreadcrumbs,
                    title: publication.title
                      ? `Publish "${truncate(publication.title, { length: 46 })}" product`
                      : t("publishNoTitle"),
                    type: "publication-publish" as OperationType,
                    publicationId: publication.id,
                    redirectAfterProposalCreation: routes.moderation.publications(storeId!, "proposals"),
                    redirectAfterProposalExecution: routes.moderation.publications(storeId!, "unpublished"),
                    invalidateQueryKeys: unpublishedPublicationsKeys.all(storeId!),
                  }}
                >
                  <ButtonPrimary className="h-11 w-40 capitalize" label={t("common:publish")} />
                </Link>
                <Link
                  to={routes.moderation.preview(storeId!)}
                  state={{
                    publicationId: publication.id,
                    previousPath: routes.moderation.unpublishedPublication(storeId!, publication.id),
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
          storeId={storeId!}
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
