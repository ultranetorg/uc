import { useCallback, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"
import { Link, useNavigate } from "react-router-dom"

import { useOperationPolicy, useStoreContext, useStoreRolesContext } from "app"
import { SvgEyeSm } from "assets"
import { useGetChangedPublication } from "entities"
import { useTransactMutationWithStatus } from "entities/iccpNode"
import { useParams, useResolveStoreId, useStoreTitle } from "hooks"
import { BaseVotableOperation, ProposalCreation, ProposalOption, Role } from "types"
import { ModerationHeader, ModerationPublicationHeader, ProductFieldsDiff } from "ui/components/specific"
import { ButtonBar, ButtonOutline, ButtonPrimary } from "ui/components"
import { routes, showToast } from "utils"

export const ModeratorChangedPublicationPage = () => {
  const navigate = useNavigate()
  const { publicationId } = useParams()
  const storeId = useResolveStoreId()
  const { t } = useTranslation()

  const { voterId } = useOperationPolicy("publication-updation")
  const { store: site } = useStoreContext()
  const { isModerator } = useStoreRolesContext()
  const { mutate } = useTransactMutationWithStatus()

  const [isPending, setPending] = useState<boolean | undefined>()

  const { isLoading, data: publication } = useGetChangedPublication(storeId, publicationId)

  const pageTitle = publication?.title ?? publication?.id
  useStoreTitle(site?.title, pageTitle ? `Changed Publication - ${pageTitle}` : "Changed Publication")

  const parentBreadcrumbs = useMemo(
    () => [{ title: t("common:publications"), path: routes.moderation.publications(storeId!, "changed") }],
    [storeId, t],
  )

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

    const operation = new ProposalCreation(storeId!, voterId!, role, "", options, "")
    mutate(operation, {
      onSuccess: () => {
        showToast(t("toast:publicationUpdated"), "success")
        navigate(routes.moderation.publications(storeId!))
      },
      onError: err => showToast(err.toString(), "error"),
    })
  }, [isModerator, mutate, navigate, publication, storeId, t, voterId])

  if (!storeId || isLoading) return <div>Loading</div>

  if (!publication) {
    return <div>Not found</div>
  }

  return (
    <div className="flex flex-col gap-6">
      <ModerationHeader
        title={publication.id}
        parentBreadcrumbs={parentBreadcrumbs}
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
                  to={routes.moderation.preview(storeId!)}
                  state={{
                    publicationId: publication.id,
                    previousPath: routes.moderation.changedPublication(storeId!, publication.id),
                    parentBreadcrumbs,
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

      <div className="flex flex-col gap-6 rounded-lg bg-gray-100 p-6">
        <ModerationPublicationHeader
          siteId={storeId}
          title={publication.title}
          logoId={publication.logoId}
          authorId={publication.authorId}
          authorTitle={publication.authorTitle}
        />
        <ProductFieldsDiff from={publication.fields} to={publication.fieldsTo} />
      </div>
    </div>
  )
}
