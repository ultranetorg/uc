import { memo, useCallback } from "react"
import { Navigate, useLocation, useNavigate, useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { Controller, useFormContext } from "react-hook-form"
import { useQueryClient } from "@tanstack/react-query"

import { twMerge } from "tailwind-merge"
import { useModerationContext, useSiteContext, useUserContext } from "app"
import { useTransactMutationWithStatus } from "entities/node"
import { CreateProposalData, ProposalType, Role } from "types"
import { ProposalCreation } from "types/fairOperations"
import {
  BreadcrumbsItemProps,
  ButtonOutline,
  ButtonPrimary,
  Collapse,
  DebugPanel,
  Input,
  PageHeader,
  Textarea,
  ValidationWrapper,
} from "ui/components"
import { OptionsEditor } from "ui/components/proposal"
import { showToast } from "utils"

import { isVotingRequired, prepareProposalOptions } from "./utils"

const LABEL_CLASSNAME = "first-letter:uppercase font-medium leading-4 text-2xs"

export type CreateProposalViewProps = {
  proposalType: ProposalType
}

export const CreateProposalView = memo(({ proposalType }: CreateProposalViewProps) => {
  const location = useLocation()
  const navigate = useNavigate()
  const { siteId } = useParams()
  const queryClient = useQueryClient()
  const { t } = useTranslation("createProposal")

  const { isModerator, isPublisher, policies } = useModerationContext()
  const { site } = useSiteContext()
  const { user } = useUserContext()

  const {
    control,
    formState: { isValid, errors },
    handleSubmit,
    watch,
  } = useFormContext<CreateProposalData>()
  const formData: CreateProposalData = watch() // TODO: should be removed.

  const { mutate, isPending } = useTransactMutationWithStatus()

  const parentBreadcrumbs = location.state?.parentBreadcrumbs as BreadcrumbsItemProps[] | undefined
  const parentPath = proposalType === "discussion" ? `/${siteId}/m` : `/${siteId}/g/r`
  const isRequiredVoting = isVotingRequired(proposalType, site, formData.type, policies)

  const handleCancelClick = useCallback(() => navigate(-1), [navigate])

  const handleFormSubmit = (data: CreateProposalData) => {
    const options = prepareProposalOptions(data)

    const by = proposalType === "discussion" ? user!.id : user!.authorsIds[0]
    const role = proposalType === "discussion" ? Role.Moderator : Role.Publisher
    const operation = new ProposalCreation(siteId!, by, role, data.title, options, data.description)
    mutate(operation, {
      onSuccess: () => {
        if (Array.isArray(location.state?.invalidateQueryKeys)) {
          queryClient.invalidateQueries({ queryKey: location.state.invalidateQueryKeys })
        }

        const translationKey = isRequiredVoting ? "toast:proposalCreated" : "toast:operationExecuted"
        showToast(t(translationKey, { operation: t(`operations:${data.type}`) }), "success")
        navigate(formData.previousPath !== undefined ? formData.previousPath : parentPath)
      },
      onError: err => {
        showToast(err.toString(), "error")
      },
    })
  }

  const validRole = (proposalType === "discussion" && isModerator) || (proposalType === "referendum" && isPublisher)
  if (!user || !validRole) {
    return <Navigate to={parentPath} />
  }

  const title = isRequiredVoting
    ? proposalType === "discussion"
      ? t("createDiscussion")
      : t("createReferendum")
    : t("common:ok")

  if (!formData.type) return <Navigate to={`/${siteId}`} />

  return (
    <div className="flex max-w-[648px] flex-col gap-6">
      <PageHeader
        siteId={siteId!}
        homeLabel={t("common:home")}
        title={
          (location.state?.title as string)
            ? location.state?.title
            : proposalType === "discussion"
              ? t("createDiscussion")
              : t("createReferendum")
        }
        parentBreadcrumbs={
          parentBreadcrumbs
            ? parentBreadcrumbs
            : {
                title: t(`${proposalType === "discussion" ? "moderation" : "governance"}:title`),
                path: parentPath,
              }
        }
      />

      <form className="flex flex-col gap-6" onSubmit={handleSubmit(handleFormSubmit)}>
        <div className="flex flex-col gap-2">
          <span className={LABEL_CLASSNAME}>{t("common:title")}:</span>
          <Controller
            control={control}
            name="title"
            rules={{ required: t("validation:requiredTitle") }}
            render={({ field, fieldState }) => (
              <ValidationWrapper message={fieldState.error?.message}>
                <Input
                  value={field.value}
                  error={!!fieldState.error?.message}
                  onChange={field.onChange}
                  onBlur={field.onBlur}
                />
              </ValidationWrapper>
            )}
          />
        </div>
        <Collapse className={LABEL_CLASSNAME} text={`${t("description")}:`}>
          <Controller
            control={control}
            name="description"
            render={({ field }) => <Textarea onChange={field.onChange} value={field.value} />}
          />
        </Collapse>

        <OptionsEditor t={t} labelClassName={LABEL_CLASSNAME} isVotingRequired={isRequiredVoting} />
        <DebugPanel data={formData} />

        <div className="flex items-center justify-end gap-6">
          <ButtonOutline label={t("common:cancel")} className="h-11 w-25" onClick={handleCancelClick} />
          <ButtonPrimary
            label={title}
            className={twMerge("h-11 w-42.5", !isRequiredVoting && "w-25 uppercase")}
            disabled={!isValid || Object.keys(errors).length > 0 || isPending}
            type="submit"
          />
        </div>
      </form>
    </div>
  )
})
