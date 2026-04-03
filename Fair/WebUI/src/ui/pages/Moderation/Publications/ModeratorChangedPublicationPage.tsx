import { useCallback } from "react"
import { useTranslation } from "react-i18next"
import { Link, useNavigate, useParams } from "react-router-dom"

import { useModerationContext } from "app"
import { SvgEyeSm } from "assets"
import { useGetChangedPublication } from "entities"
import { useTransactMutationWithStatus } from "entities/node"
import { PageType } from "types"
import { ModerationHeader, ProductFieldsDiff } from "ui/components/specific"
import { ButtonBar, ButtonOutline, ButtonPrimary } from "ui/components"

export const ModeratorChangedPublicationPage = () => {
  const { siteId, publicationId } = useParams()
  const { getOperationVoterId } = useModerationContext()
  const { mutate } = useTransactMutationWithStatus()
  const navigate = useNavigate()
  const { t } = useTranslation()

  const voterId = getOperationVoterId("publication-updation")

  const { isLoading, data: publication } = useGetChangedPublication(siteId, publicationId)

  if (!siteId || isLoading) return <div>LOADING</div>

  // Backend may return an error payload without a `publication` field (e.g. 404),
  // so guard against missing nested data before accessing it.
  if (!publication || !publication.publication) {
    return <div>NOT FOUND</div>
  }

  return (
    <div className="flex flex-col gap-6">
      <ModerationHeader
        title={publication.publication.title}
        //logoFileId={publication.publication.imageId}
        parentBreadcrumbs={[
          { title: t("common:proposals"), path: `/${siteId}/m` },
          { title: t("common:publications"), path: `/${siteId}/m/c` },
        ]}
        components={
          <>
            {!!voterId && (
              /*voteStatus !== "voted" &&*/ <ButtonBar className="items-center">
                <ButtonPrimary
                  className="h-11 w-43.75"
                  label="Update publication"
                  //onClick={handleApprove}
                  // disabled={voteStatus === "voting"}
                  // loading={voteAction === "approve"}
                />
                {/* <Link
                  to={`/${siteId}/m/v`}
                  state={{
                    productId: productId,
                    publicationId: publicationId,
                    proposalId: proposal?.id,
                    source: "ModeratorChangedPublicationPage" as PageType,
                  }}
                > */}
                <ButtonOutline
                  //disabled={voteStatus === "voting"}
                  className="h-11 w-52"
                  label="Preview publication"
                  iconBefore={<SvgEyeSm className="fill-gray-800" />}
                />
                {/* </Link> */}
              </ButtonBar>
            )}
          </>
        }
      />

      {/* TODO: ProductCompareFields component should be modified and receive product.versions property instead of productIds to avoid second request to back-end. */}
      <ProductFieldsDiff from={publication.from} to={publication.to} />
    </div>
  )
}
