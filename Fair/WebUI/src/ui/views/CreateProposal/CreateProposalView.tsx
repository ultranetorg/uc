import { memo, useCallback } from "react"
import { Navigate, useLocation, useNavigate } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { Controller, useFormContext } from "react-hook-form"
import { useQueryClient } from "@tanstack/react-query"
import { twMerge } from "tailwind-merge"

import { useStoreContext, useStorePoliciesContext, useStoreRolesContext, useUserContext } from "app"
import { PROPOSAL_TEXT_MAX_LENGTH, PROPOSAL_TITLE_MAX_LENGTH } from "constants/"
import { useTransactMutationWithStatus } from "entities/iccpNode"
import { useResolveStoreId } from "hooks"
import { CreateProposalData, ProposalCreation, ProposalType, Role } from "types"
import {
  BreadcrumbsItemProps,
  ButtonOutline,
  ButtonPrimary,
  Collapse,
  DebugPanel,
  Input,
  MessageBox,
  PageHeader,
  Textarea,
  ValidationWrapper,
} from "ui/components"
import { OptionsEditor } from "ui/components/proposal"
import { isArrayOfArrays, isVotingRequired, routes, showToast } from "utils"

import { prepareProposalOptions } from "./utils"

const LABEL_CLASSNAME = "first-letter:uppercase font-medium leading-4 text-2xs"

export type CreateProposalViewProps = {
  proposalType: ProposalType
}

export const CreateProposalView = memo(({ proposalType }: CreateProposalViewProps) => {
  const location = useLocation()
  const navigate = useNavigate()
  const storeId = useResolveStoreId()
  const queryClient = useQueryClient()
  const { t } = useTranslation("createProposal")

  const { isModerator, isPublisher } = useStoreRolesContext()
  const { policies } = useStorePoliciesContext()
  const { store: site } = useStoreContext()
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
  const parentPath =
    proposalType === "discussion" ? routes.moderation.proposals(storeId!) : routes.governance.referendums(storeId!)
  const isRequiredVoting = isVotingRequired(formData.type, site, policies)

  const handleCancelClick = useCallback(() => navigate(-1), [navigate])

  const handleFormSubmit = (data: CreateProposalData) => {
    const options = prepareProposalOptions(data)

    const by = proposalType === "discussion" ? user!.id : user!.authorsIds[0]
    const role = proposalType === "discussion" ? Role.Moderator : Role.Publisher
    const operation = new ProposalCreation(storeId!, by, role, data.title, options, data.description)
    mutate(operation, {
      onSuccess: () => {
        if (!isRequiredVoting && Array.isArray(location.state?.invalidateQueryKeys)) {
          if (isArrayOfArrays(location.state.invalidateQueryKeys)) {
            location.state.invalidateQueryKeys.each((x: readonly unknown[]) =>
              queryClient.invalidateQueries({ queryKey: x }),
            )
          } else {
            queryClient.invalidateQueries({ queryKey: location.state.invalidateQueryKeys })
          }
        }

        const translationKey = isRequiredVoting ? "toast:proposalCreated" : "toast:proposalExecuted"
        showToast(t(translationKey, { operation: t(`operations:${data.type}`) }), "success")

        const navigateTo = isRequiredVoting
          ? location.state.redirectAfterProposalCreation
          : location.state.redirectAfterProposalExecution
        navigate(navigateTo)
      },
      onError: err => {
        showToast(t(err.message), "error")
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

  if (
    !formData.type ||
    !location.state?.redirectAfterProposalCreation ||
    !location.state?.redirectAfterProposalExecution
  ) {
    return <Navigate to={routes.store(storeId!)} />
  }

  return (
    <div className="flex max-w-[648px] flex-col gap-6">
      <PageHeader
        storeId={storeId!}
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
            rules={{
              required: t("validation:requiredTitle"),
              maxLength: {
                value: PROPOSAL_TITLE_MAX_LENGTH,
                message: t("validation:maxLength", { count: PROPOSAL_TITLE_MAX_LENGTH }),
              },
            }}
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
            rules={{
              maxLength: {
                value: PROPOSAL_TEXT_MAX_LENGTH,
                message: t("validation:maxLength", { count: PROPOSAL_TEXT_MAX_LENGTH }),
              },
            }}
            render={({ field }) => <Textarea onChange={field.onChange} value={field.value} />}
          />
        </Collapse>

        <OptionsEditor t={t} labelClassName={LABEL_CLASSNAME} isVotingRequired={isRequiredVoting} />
        {isRequiredVoting && <MessageBox message={t("addedAnswers")} type="warning" />}
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
