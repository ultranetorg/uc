import { memo, PropsWithChildren } from "react"
import { useTranslation } from "react-i18next"
import { twMerge } from "tailwind-merge"

import { Policy, Proposal, PropsWithClassName, Store } from "types"
import { LinkFullscreen } from "ui/components"
import { renderAction, renderLeadingOption, renderProposalProgress, renderYourVote } from "ui/renderers/utils"
import { renderCreator } from "ui/renderers2"
import { buildCreatorLink, getLastsFor, formatDate, calculateVotesRequiredToWinProposal } from "utils"

type InfoRowBaseProps = {
  label: string
}

type InfoRowProps = PropsWithChildren & InfoRowBaseProps

const InfoRow = ({ label, children }: InfoRowProps) => (
  <div className="flex flex-col gap-2">
    <span className="text-gray-500">{label}</span>
    {children}
  </div>
)

type ProposalInfoBaseProps = {
  store: Store
  proposal: Proposal
  policies: Policy[]
  voterIds: string[]
}

export type ProposalInfoProps = ProposalInfoBaseProps & PropsWithClassName

export const ProposalInfo = memo(({ className, store, proposal, policies, voterIds }: ProposalInfoProps) => {
  const { t } = useTranslation()

  const requiredToWin = calculateVotesRequiredToWinProposal(proposal.operation, store, policies)

  return (
    <div
      className={twMerge(
        "flex max-h-fit flex-col gap-4 rounded-lg border border-gray-300 bg-gray-100 p-6 text-2sm leading-5 text-gray-800",
        className,
      )}
    >
      <InfoRow label={t("labels:createdBy")}>
        <LinkFullscreen to={buildCreatorLink(store.id, proposal.creator)}>
          {renderCreator(proposal.creator)}
        </LinkFullscreen>
      </InfoRow>
      <InfoRow label={t("labels:createdAt")}>
        <span>{formatDate(proposal.creationTime)}</span>
      </InfoRow>
      <InfoRow label={t("labels:action")}>{renderAction(t, proposal.operation)}</InfoRow>
      <InfoRow label={t("labels:leadingOption")}>{renderLeadingOption(t, proposal)}</InfoRow>
      <InfoRow label={t("labels:progress")}>{renderProposalProgress(t, proposal, requiredToWin)}</InfoRow>
      <InfoRow label={t("labels:yourVote")}>{renderYourVote(t, proposal, voterIds)}</InfoRow>
      <InfoRow label={t("labels:openFor")}>
        <span>{getLastsFor(t, proposal.creationTime)}</span>
      </InfoRow>
    </div>
  )
})
