import { memo, useCallback, useMemo, useState } from "react"
import { useParams, useSearchParams } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { twMerge } from "tailwind-merge"
import { pick } from "lodash"

import { CREATE_PROPOSAL_DURATIONS, CREATE_PROPOSAL_DURATION_DEFAULT } from "constants/"
import { CreateProposalData, CreateProposalDataOption, OperationType, ProposalType } from "types"
import {
  ButtonOutline,
  ButtonPrimary,
  DebugPanel,
  Dropdown,
  DropdownItem,
  Input,
  PageHeader,
  Textarea,
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
  const [searchParams] = useSearchParams()

  const [data, setData] = useState<CreateProposalData>({
    title: "",
    duration: CREATE_PROPOSAL_DURATION_DEFAULT,
    ...(searchParams.get("type") && { type: searchParams.get("type")! as OperationType }),
    ...(searchParams.get("productId") && { productId: searchParams.get("productId")! }),
    ...(searchParams.get("publicationId") && { publicationId: searchParams.get("publicationId")! }),
    ...(searchParams.get("reviewId") && { reviewId: searchParams.get("reviewId")! }),
    ...(searchParams.get("userId") && { userId: searchParams.get("userId")! }),
  })

  const title = proposalType === "discussion" ? t("createDiscussion") : t("createReferendum")

  const durationItems = useMemo<DropdownItem[]>(() => CREATE_PROPOSAL_DURATIONS.map(x => ({ label: x, value: x })), [])

  const handleDataChange = useCallback(
    (name: string, value: string | CreateProposalDataOption[] | undefined) =>
      setData(p => {
        const base = name === "type" ? pick(p, ["title", "description", "duration"]) : p
        return { ...base, [name]: value }
      }),
    [],
  )

  const handleDataOptionChange = useCallback(
    (index: number, name: string, value: string | string[]) =>
      setData(p => ({ ...p, options: p.options!.map((x, i) => (i !== index ? x : { ...x, [name]: value })) })),
    [],
  )

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
            <Input onChange={value => handleDataChange("title", value)} value={data.title} />
          </div>
          <div className="flex flex-col gap-2">
            <span className={LABEL_CLASSNAME}>{t("description")}:</span>
            <Textarea onChange={value => handleDataChange("description", value)} value={data.description} />
          </div>
          <div className="flex flex-col gap-2">
            <span className={LABEL_CLASSNAME}>{t("howManyDays")}:</span>
            <Dropdown
              isMulti={false}
              items={durationItems}
              defaultValue={CREATE_PROPOSAL_DURATION_DEFAULT.toString()}
              size="large"
              onChange={item => handleDataChange("duration", item.value)}
            />
          </div>
        </>
      )}

      <OptionsEditor
        t={t}
        proposalType={proposalType}
        labelClassName={LABEL_CLASSNAME}
        requiresVoting={requiresVoting}
        data={data}
        onDataChange={handleDataChange}
        onDataOptionChange={handleDataOptionChange}
      />
      <DebugPanel data={data} />

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
