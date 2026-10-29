import { MouseEvent, ReactNode } from "react"
import { TFunction } from "i18next"
import { twMerge } from "tailwind-merge"

import { SvgLock } from "assets"
import { ApprovalRequirement, Policy, StoreApprovalPolicyChange, Survey } from "types"
import { NotVotedBadge, Tooltip, VotesProgress } from "ui/components"

export const CELL_CLASSNAME = "flex items-center gap-2"

export const stopPropagation = (e: MouseEvent) => e.stopPropagation()

export const getOperation = (survey: Survey, index: number): StoreApprovalPolicyChange | undefined =>
  survey?.options && survey.options.length > 0 && index < survey.options.length
    ? (survey.options[index].operation as StoreApprovalPolicyChange)
    : undefined

export const getCurrentApproval = (survey: Survey, policies?: Policy[]): ApprovalRequirement | undefined => {
  if (survey.lastWin === -1) {
    const operation = getOperation(survey, 0)
    if (!operation || !policies) return undefined

    return policies.find(x => x.operationClass === operation.operation)?.approval as ApprovalRequirement
  }

  return getOperation(survey, survey.lastWin)?.approval as ApprovalRequirement
}

export const getLeadingVotes = (survey: Survey) => Math.max(0, ...survey.options.map(x => x.yes.length))

export const getLeadingApprovals = (survey: Survey): ApprovalRequirement[] => {
  const leadingVotes = getLeadingVotes(survey)
  if (leadingVotes === 0) return []

  return survey.options
    .filter(x => x.yes.length === leadingVotes)
    .map(x => (x.operation as StoreApprovalPolicyChange).approval as ApprovalRequirement)
}

export const getPublisherVotedApproval = (survey: Survey, publisherIds: string[]): ApprovalRequirement | undefined => {
  const votedOption = survey.options.find(x => x.yes.some(id => publisherIds.includes(id)))

  return votedOption
    ? ((votedOption.operation as StoreApprovalPolicyChange).approval as ApprovalRequirement)
    : undefined
}

export const renderVotedApproval = (t: TFunction, survey: Survey, policies?: Policy[]): ReactNode => {
  const approval = getCurrentApproval(survey, policies)
  if (!approval) return "-"

  const text = t(`approvalRequirement:${approval}`)
  return survey.isVariableApproval ? (
    text
  ) : (
    <div className={CELL_CLASSNAME}>
      <Tooltip text={t("unchangeablePolicy")}>
        <SvgLock fill="fill-gray-800" className="cursor-default" onClick={stopPropagation} />
      </Tooltip>
      {text}
    </div>
  )
}

export const renderLeadingOption = (t: TFunction, survey: Survey, policies?: Policy[]): ReactNode => {
  const leaders = getLeadingApprovals(survey)
  if (leaders.length === 0) return "-"

  const text = leaders.map(x => t(`approvalRequirement:${x}`)).join(", ")

  const isCurrent = leaders.length === 1 && leaders[0] === getCurrentApproval(survey, policies)
  return <span className={twMerge(isCurrent && "text-gray-500")}>{text}</span>
}

export const renderProgress = (survey: Survey): ReactNode => (
  <VotesProgress votes={getLeadingVotes(survey)} votesRequired={survey.votesRequiredToWin} />
)

export const renderYourVote = (t: TFunction, survey: Survey, publisherIds?: string[]): ReactNode => {
  if (!publisherIds || publisherIds.length === 0) return "-"

  const approval = getPublisherVotedApproval(survey, publisherIds)
  if (!approval) {
    return <NotVotedBadge />
  }

  return t(`approvalRequirement:${approval}`)
}
