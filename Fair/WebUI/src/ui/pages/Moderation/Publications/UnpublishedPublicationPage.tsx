import { useCallback } from "react"
import { Link, useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"

import { useModerationContext } from "app"
import { SvgEyeSm } from "assets"
import { useGetUnpublishedPublication } from "entities"
import { ModerationHeader, ProductFieldsTree } from "ui/components/specific"
import { ButtonBar, ButtonOutline, ButtonPrimary } from "ui/components"

export const UnpublishedPublicationPage = () => {
  const { siteId, publicationId } = useParams()
  const { getOperationVoterId } = useModerationContext()
  const { t } = useTranslation()

  const voterId = getOperationVoterId("publication-updation")

  const { isLoading, data: publication } = useGetUnpublishedPublication(siteId, publicationId)

  const handlePublishPublication = useCallback(() => alert("preview"), [])

  if (isLoading || !publication) return <div>LOADING</div>

  return (
    <div className="flex flex-col gap-6">
      <ModerationHeader
        title={publication.title}
        // TODO: show id
        //logoFileId={publication.publication.imageId}
        parentBreadcrumbs={[
          { title: t("common:proposals"), path: `/${siteId}/m` },
          { title: t("common:publications"), path: `/${siteId}/m/c` },
        ]}
        components={
          <>
            {!!voterId && (
              <ButtonBar className="items-center">
                <ButtonPrimary className="h-11 w-43.75" label="Publish" onClick={handlePublishPublication} />
                <Link
                  to={`/${siteId}/m/v`}
                  state={{
                    publicationId: publication.id,
                    previousPath: `/${siteId}/m/c/u/${publication.id}`,
                  }}
                >
                  <ButtonOutline
                    className="h-11 w-52"
                    label="Preview publication"
                    iconBefore={<SvgEyeSm className="fill-gray-800" />}
                  />
                </Link>
              </ButtonBar>
            )}
          </>
        }
      />

      <ProductFieldsTree productFields={publication.fields} />
    </div>
  )
}
