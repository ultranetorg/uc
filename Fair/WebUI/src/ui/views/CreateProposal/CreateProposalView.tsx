import { memo, useCallback, useMemo, useState } from "react"
import { useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"

import {
  ButtonOutline,
  ButtonPrimary,
  DropdownItem,
  DropdownPrimary,
  Input,
  MessageBox,
  PageHeader,
  Textarea,
} from "ui/components"
import { OptionsEditorList } from "ui/components/proposal"

import {
  OPERATION_TYPES,
  PROPOSAL_DURATION_DEFAULT,
  PROPOSAL_DURATIONS,
  SINGLE_OPTION_OPERATION_TYPES,
} from "./constants"
import { OperationType } from "types"
import { twMerge } from "tailwind-merge"

const LABEL_CLASSNAME = "first-letter:uppercase font-medium leading-4 text-2xs"

export type CreateProposalViewProps = {
  proposalType: "discussion" | "referendum"
  requiresVoting?: boolean
}

export const CreateProposalView = memo(({ proposalType, requiresVoting = true }: CreateProposalViewProps) => {
  const { t } = useTranslation("createProposal")
  const { siteId } = useParams()

  const [operationType, setOperationType] = useState<OperationType | undefined>()

  const title = proposalType === "discussion" ? t("createDiscussion") : t("createReferendum")
  const singleOption = !requiresVoting || (!!operationType && SINGLE_OPTION_OPERATION_TYPES.includes(operationType))

  const durationItems = useMemo<DropdownItem[]>(() => PROPOSAL_DURATIONS.map(x => ({ label: x, value: x })), [])

  const typesItems = useMemo<DropdownItem[]>(
    () => OPERATION_TYPES.map(x => ({ label: t(`operations:${x}`), value: x as string })),
    [t],
  )

  const handleTypeChange = useCallback((item: DropdownItem) => setOperationType(item.value as OperationType), [])

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

      {requiresVoting && (
        <>
          <div className="flex flex-col gap-2">
            <span className={LABEL_CLASSNAME}>{t("common:title")}:</span>
            <Input onChange={console.log} />
          </div>
          <div className="flex flex-col gap-2">
            <span className={LABEL_CLASSNAME}>{t("description")}:</span>
            <Textarea onChange={console.log} />
          </div>
          <div className="flex flex-col gap-2">
            <span className={LABEL_CLASSNAME}>{t("howManyDays")}:</span>
            <DropdownPrimary items={durationItems} defaultValue={PROPOSAL_DURATION_DEFAULT} />
          </div>
        </>
      )}

      <div className="flex flex-col gap-2">
        <span className={LABEL_CLASSNAME}>{t("common:type")}:</span>
        <DropdownPrimary items={typesItems} onChange={handleTypeChange} />
      </div>
      <div className="flex flex-col gap-4">
        {!singleOption && (
          <span className="text-xl font-semibold leading-6 first-letter:uppercase">{t("common:options")}:</span>
        )}
        <OptionsEditorList t={t} operationType={operationType} singleOption={singleOption} />
      </div>
      <MessageBox message={t("addedAnswers")} type="warning" />
      <div className="flex items-center justify-end gap-6">
        <ButtonOutline label={t("common:cancel")} className="h-11 w-25" />
        <ButtonPrimary
          label={requiresVoting ? title : t("common:ok")}
          className={twMerge("h-11 w-42.5", !requiresVoting && "uppercase")}
        />
      </div>
    </div>
  )
})
