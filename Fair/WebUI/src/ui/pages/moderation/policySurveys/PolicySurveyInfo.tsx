import { memo, PropsWithChildren } from "react"
import { useTranslation } from "react-i18next"
import { twMerge } from "tailwind-merge"

import { Policy, PropsWithClassName, SurveyDetails } from "types"

import { renderLeadingOption, renderProgress, renderVotedApproval, renderYourVote } from "./utils"

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

type PolicySurveyInfoBaseProps = {
  survey: SurveyDetails
  policies?: Policy[]
  publisherIds?: string[]
}

export type PolicySurveyInfoProps = PolicySurveyInfoBaseProps & PropsWithClassName

export const PolicySurveyInfo = memo(({ className, survey, policies, publisherIds }: PolicySurveyInfoProps) => {
  const { t } = useTranslation("policySurveysPage")

  return (
    <div
      className={twMerge(
        "flex max-h-fit flex-col gap-4 rounded-lg border border-gray-300 bg-gray-100 p-6 text-2sm leading-5 text-gray-800",
        className,
      )}
    >
      <InfoRow label={t("labels:currentPolicy")}>{renderVotedApproval(t, survey, policies)}</InfoRow>
      <InfoRow label={t("labels:leadingOption")}>{renderLeadingOption(t, survey, policies)}</InfoRow>
      <InfoRow label={t("labels:progress")}>{renderProgress(survey)}</InfoRow>
      <InfoRow label={t("labels:totalVotes")}>
        <span>{survey.totalVotes}</span>
      </InfoRow>
      <InfoRow label={t("labels:yourVote")}>{renderYourVote(t, survey, publisherIds)}</InfoRow>
      <span className="text-2xs leading-4 text-gray-500">{t("votingHint", { count: survey.votesRequiredToWin })}</span>
    </div>
  )
})
