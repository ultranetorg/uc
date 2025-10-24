import { memo, useMemo } from "react"
import { useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { twMerge } from "tailwind-merge"
import { Controller, useFormContext } from "react-hook-form"

import { CREATE_PROPOSAL_DURATIONS } from "constants/"
import { CreateProposalData, ProposalType } from "types"
import {
  ButtonOutline,
  ButtonPrimary,
  DebugPanel,
  Dropdown,
  DropdownItem,
  Input,
  PageHeader,
  Textarea,
  ValidationWrapper,
} from "ui/components"
import { OptionsEditor } from "ui/components/proposal"

const LABEL_CLASSNAME = "first-letter:uppercase font-medium leading-4 text-2xs"

export type CreateProposalViewProps = {
  proposalType: ProposalType
  requiresVoting?: boolean
}

export const CreateProposalView = memo(({ proposalType, requiresVoting = true }: CreateProposalViewProps) => {
  const { t } = useTranslation("createProposal")
  const { siteId } = useParams()

  const {
    control,
    formState: { isValid },
    handleSubmit,
    watch,
  } = useFormContext<CreateProposalData>()
  const formData: CreateProposalData = watch() // TODO: should be removed.

  const title = proposalType === "discussion" ? t("createDiscussion") : t("createReferendum")

  const durationItems = useMemo<DropdownItem[]>(() => CREATE_PROPOSAL_DURATIONS.map(x => ({ label: x, value: x })), [])

  const handleFormSubmit = (data: CreateProposalData) => {
    alert("Form submitted:" + JSON.stringify(data))
  }

  return (
    <div className="flex max-w-[648px] flex-col gap-6">
      <PageHeader
        siteId={siteId!}
        homeLabel={t("common:home")}
        title={proposalType === "discussion" ? t("createDiscussion") : t("createReferendum")}
        parentBreadcrumb={{
          title: t(`${proposalType === "discussion" ? "moderation" : "governance"}:title`),
          path: `/${siteId}/${proposalType === "discussion" ? "m" : "g"}`,
        }}
      />

      <form className="flex flex-col gap-6" onSubmit={handleSubmit(handleFormSubmit)}>
        {requiresVoting && (
          <>
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
            <div className="flex flex-col gap-2">
              <span className={LABEL_CLASSNAME}>{t("howManyDays")}:</span>
              <Controller
                control={control}
                name="duration"
                render={({ field }) => (
                  <Dropdown
                    isMulti={false}
                    controlled={true}
                    items={durationItems}
                    size="large"
                    value={field.value}
                    onChange={item => field.onChange(item.value)}
                  />
                )}
              />
            </div>
          </>
        )}

        <OptionsEditor
          t={t}
          proposalType={proposalType}
          labelClassName={LABEL_CLASSNAME}
          requiresVoting={requiresVoting}
        />
        <DebugPanel data={formData} />

        <div className="flex items-center justify-end gap-6">
          <ButtonOutline label={t("common:cancel")} className="h-11 w-25" />
          <ButtonPrimary
            label={requiresVoting ? title : t("common:ok")}
            className={twMerge("h-11 w-42.5", !requiresVoting && "uppercase")}
            disabled={!isValid}
            type="submit"
          />
        </div>
      </form>
    </div>
  )
})
