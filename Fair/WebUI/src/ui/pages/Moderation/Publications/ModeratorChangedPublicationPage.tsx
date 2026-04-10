import { useCallback, useState } from "react"
import { useTranslation } from "react-i18next"
import { Link, useNavigate, useParams } from "react-router-dom"

import { useModerationContext } from "app"
import { SvgEyeSm } from "assets"
import { useGetChangedPublication } from "entities"
import { useTransactMutationWithStatus } from "entities/node"
import { BaseVotableOperation, ProposalCreation, ProposalOption, Role } from "types"
import { ModerationHeader, ModerationPublicationHeader, ProductFieldsDiff } from "ui/components/specific"
import { ButtonBar, ButtonOutline, ButtonPrimary } from "ui/components"
import { showToast } from "utils"

export const ModeratorChangedPublicationPage = () => {
  const { siteId, publicationId } = useParams()
  const { getOperationVoterId, isModerator } = useModerationContext()
  const { mutate } = useTransactMutationWithStatus()
  const navigate = useNavigate()
  const { t } = useTranslation()

  const [isPending, setPending] = useState<boolean | undefined>()

  const voterId = getOperationVoterId("publication-updation")

  const { isLoading, data: publication } = useGetChangedPublication(siteId, publicationId)

  const handleUpdatePublication = useCallback(() => {
    setPending(true)
    const role = isModerator ? Role.Moderator : Role.Publisher
    const options = [
      {
        title: "",
        operation: {
          $type: "PublicationUpdation",
          Publication: publication!.id,
          Version: publication!.latestVersion,
        } as unknown as BaseVotableOperation,
      },
    ] as ProposalOption[]

    const operation = new ProposalCreation(siteId!, voterId!, role, "", options, "")
    mutate(operation, {
      onSuccess: () => {
        showToast(t("toast:publicationUpdated"), "success")
        navigate(`/${siteId}/m/c`)
      },
      onError: err => showToast(err.toString(), "error"),
    })
  }, [isModerator, mutate, navigate, publication, siteId, t, voterId])

  if (!siteId || isLoading) return <div>LOADING</div>

  if (!publication) {
    return <div>NOT FOUND</div>
  }

  return (
    <div className="flex flex-col gap-6">
      <ModerationHeader
        title={publication.id}
        parentBreadcrumbs={[
          { title: t("common:proposals"), path: `/${siteId}/m` },
          { title: t("common:publications"), path: `/${siteId}/m/c` },
          { title: t("common:changed"), path: `/${siteId}/m/c/c` },
        ]}
        components={
          <>
            {!!voterId && (
              <ButtonBar className="items-center">
                <ButtonPrimary
                  className="h-11 w-43.75"
                  label="Update publication"
                  onClick={handleUpdatePublication}
                  disabled={!!isPending}
                  loading={!!isPending}
                />
                <Link
                  to={`/${siteId}/m/v`}
                  state={{
                    publicationId: publication.id,
                    previousPath: `/${siteId}/m/c/c/${publication.id}`,
                  }}
                >
                  <ButtonOutline
                    disabled={!!isPending}
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
      <ProductFieldsDiff from={publication.fields} to={publication.fieldsTo} />
    </div>
  )
}
