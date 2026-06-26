import { useCallback, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"
import { Link, useNavigate } from "react-router-dom"

import { useOperationPolicy, useSiteContext, useSiteRolesContext } from "app"
import { SvgEyeSm } from "assets"
import { useGetChangedPublication } from "entities"
import { useTransactMutationWithStatus } from "entities/iccpNode"
import { useParams, useResolveSiteId, useSiteTitle } from "hooks"
import { BaseVotableOperation, ProposalCreation, ProposalOption, Role } from "types"
import { ModerationHeader, ModerationPublicationHeader, ProductFieldsDiff } from "ui/components/specific"
import { ButtonBar, ButtonOutline, ButtonPrimary } from "ui/components"
import { routes, showToast } from "utils"

export const ModeratorChangedPublicationPage = () => {
  const navigate = useNavigate()
  const { publicationId } = useParams()
  const siteId = useResolveSiteId()
  const { t } = useTranslation()

  const { voterId } = useOperationPolicy("publication-updation")
  const { site } = useSiteContext()
  const { isModerator } = useSiteRolesContext()
  const { mutate } = useTransactMutationWithStatus()

  const [isPending, setPending] = useState<boolean | undefined>()

  const { isLoading, data: publication } = useGetChangedPublication(siteId, publicationId)

  const pageTitle = publication?.title ?? publication?.id
  useSiteTitle(site?.title, pageTitle ? `Changed Publication - ${pageTitle}` : "Changed Publication")

  const parentBreadcrumbs = useMemo(
    () => [
      { title: t("common:publications"), path: routes.moderation.publications(siteId!) },
      { title: t("common:changed"), path: routes.moderation.publications(siteId!, "changed") },
    ],
    [siteId, t],
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

    const operation = new ProposalCreation(siteId!, voterId!, role, "", options, "")
    mutate(operation, {
      onSuccess: () => {
        showToast(t("toast:publicationUpdated"), "success")
        navigate(routes.moderation.publications(siteId!))
      },
      onError: err => showToast(err.toString(), "error"),
    })
  }, [isModerator, mutate, navigate, publication, siteId, t, voterId])

  if (!siteId || isLoading) return <div>Loading</div>

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
                  to={routes.moderation.preview(siteId!)}
                  state={{
                    publicationId: publication.id,
                    previousPath: routes.moderation.changedPublication(siteId!, publication.id),
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
          siteId={siteId}
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
