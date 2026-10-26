import { memo, useCallback } from "react"
import { Navigate, useLocation, useNavigate } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { Controller, useFormContext } from "react-hook-form"
import { twMerge } from "tailwind-merge"

import { useOperationPolicy, useStoreContext, useStorePoliciesContext, useUserContext } from "app"
import { PROPOSAL_TEXT_MAX_LENGTH, PROPOSAL_TITLE_MAX_LENGTH } from "constants/"
import { useInvalidation } from "entities"
import { useTransactMutationWithStatus } from "entities/iccpNode"
import { useResolveStoreId } from "hooks"
import { CreateProposalData, ProposalCreation } from "types"
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
import { isVotingRequired, routes, showToast } from "utils"

import { prepareProposalOptions } from "./utils"

const LABEL_CLASSNAME = "first-letter:uppercase font-medium leading-4 text-2xs"

export const CreateProposalView = memo(() => {
  const location = useLocation()
  const navigate = useNavigate()
  const storeId = useResolveStoreId()
  const { invalidateOperation, invalidateProposal } = useInvalidation()
  const { t } = useTranslation("createProposal")

  const policies = useStorePoliciesContext()
  const { store } = useStoreContext()
  const { user } = useUserContext()

  const {
    control,
    formState: { isValid, errors },
    handleSubmit,
    watch,
  } = useFormContext<CreateProposalData>()
  const formData: CreateProposalData = watch() // TODO: should be removed.
  const { creator } = useOperationPolicy(formData.type)

  const { mutate, isPending } = useTransactMutationWithStatus()

  const parentBreadcrumbs = location.state?.parentBreadcrumbs as BreadcrumbsItemProps[] | undefined
  const parentPath = routes.moderation.proposals(storeId!)
  const isRequiredVoting = isVotingRequired(formData.type, store, policies)

  const handleCancelClick = useCallback(() => navigate(-1), [navigate])

  const handleFormSubmit = (data: CreateProposalData) => {
    const options = prepareProposalOptions(data)

    const operation = new ProposalCreation(storeId!, creator!.id, creator!.role, data.title, options, data.description)
    mutate(operation, {
      onSuccess: () => {
        invalidateProposal({ storeId: storeId! })
        if (!isRequiredVoting) {
          invalidateOperation(data.type, {
            storeId: storeId!,
            userName: user?.name,
            publicationId: data.publicationId ?? location.state?.publicationId,
          })
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

  if (!user || !creator) {
    return <Navigate to={parentPath} />
  }

  const title = isRequiredVoting ? t("createProposal") : t("common:ok")

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
        title={(location.state?.title as string) ? location.state?.title : t("createProposal")}
        parentBreadcrumbs={parentBreadcrumbs ? parentBreadcrumbs : { title: t("moderation:title"), path: parentPath }}
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
