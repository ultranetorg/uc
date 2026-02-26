import { memo } from "react"
import { Navigate, useNavigate, useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { Controller, useFormContext } from "react-hook-form"

import { useUserContext } from "app"
import { CREATE_DISCUSSION_EXTRA_OPERATION_TYPES } from "constants/"
import { useTransactMutationWithStatus } from "entities/node"
import { CreateProposalData, ExtendedOperationType, ProposalType, Role } from "types"
import { ProposalCreation } from "types/fairOperations"
import { ButtonOutline, ButtonPrimary, DebugPanel, Input, PageHeader, Textarea, ValidationWrapper } from "ui/components"
import { OptionsEditor } from "ui/components/proposal"
import { showToast } from "utils"

import { prepareProposalOptions } from "./utils"

const LABEL_CLASSNAME = "first-letter:uppercase font-medium leading-4 text-2xs"

export type CreateProposalViewProps = {
  proposalType: ProposalType
}

export const CreateProposalView = memo(({ proposalType }: CreateProposalViewProps) => {
  const { t } = useTranslation("createProposal")
  const { siteId } = useParams()
  const navigate = useNavigate()

  const { user, isModerator, isPublisher } = useUserContext()

  const {
    control,
    formState: { isValid, errors },
    handleSubmit,
    watch,
  } = useFormContext<CreateProposalData>()
  const formData: CreateProposalData = watch() // TODO: should be removed.

  const { mutate, isPending } = useTransactMutationWithStatus()

  const parentPath = proposalType === "discussion" ? `/${siteId}/m` : `/${siteId}/g/r`

  const handleFormSubmit = (data: CreateProposalData) => {
    const options = prepareProposalOptions(data)

    const by = proposalType === "discussion" ? user!.id : user!.authorsIds[0]
    const role = proposalType === "discussion" ? Role.Moderator : Role.Publisher
    const operation = new ProposalCreation(siteId!, by, role, data.title, options, data.description)
    mutate(operation, {
      onSuccess: () => {
        showToast(
          t("toast:proposalCreated", {
            proposal: !CREATE_DISCUSSION_EXTRA_OPERATION_TYPES.includes(data.type as ExtendedOperationType)
              ? t(`operations:${data.type}`)
              : t(`extraOperations:${data.type}`),
          }),
          "success",
        )
        navigate(parentPath)
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

  const title = proposalType === "discussion" ? t("createDiscussion") : t("createReferendum")

  return (
    <div className="flex max-w-[648px] flex-col gap-6">
      <PageHeader
        siteId={siteId!}
        homeLabel={t("common:home")}
        title={proposalType === "discussion" ? t("createDiscussion") : t("createReferendum")}
        parentBreadcrumb={{
          title: t(`${proposalType === "discussion" ? "moderation" : "governance"}:title`),
          path: parentPath,
        }}
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
        <div className="flex flex-col gap-2">
          <span className={LABEL_CLASSNAME}>{t("description")}:</span>
          <Controller
            control={control}
            name="description"
            render={({ field }) => <Textarea onChange={field.onChange} value={field.value} />}
          />
        </div>

        <OptionsEditor t={t} proposalType={proposalType} labelClassName={LABEL_CLASSNAME} />
        <DebugPanel data={formData} />

        <div className="flex items-center justify-end gap-6">
          <ButtonOutline label={t("common:cancel")} className="h-11 w-25" />
          <ButtonPrimary
            label={title}
            className={"h-11 w-42.5"}
            disabled={!isValid || Object.keys(errors).length > 0 || isPending}
            type="submit"
          />
        </div>
      </form>
    </div>
  )
})
