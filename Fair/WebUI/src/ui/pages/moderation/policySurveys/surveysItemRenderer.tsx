import { ReactNode } from "react"
import { TFunction } from "i18next"

import { Survey, Policy, StoreApprovalPolicyChange } from "types"
import { TableColumn, TableItem } from "ui/components/Table"
import { formatPercents } from "utils"

const getOperation = (survey: Survey, index: number): StoreApprovalPolicyChange | undefined =>
  survey?.options && survey.options.length > 0 && index < survey.options.length
    ? (survey.options[index].operation as StoreApprovalPolicyChange)
    : undefined

const getVotedApproval = (t: TFunction, survey: Survey, policies?: Policy[]): string => {
  if (survey.lastWin === -1) {
    const operation = getOperation(survey, 0)
    if (!operation || !policies) return "-"

    const approval = policies.find(x => x.operationClass === operation.operation)
    if (!approval) return "-"

    return t(`approvalRequirement:${approval.approval}`)
  }

  const winOperation = getOperation(survey, survey.lastWin)!
  return t(`approvalRequirement:${winOperation.approval}`)
}

export const surveysItemRenderer =
  (t: TFunction, policies?: Policy[]) =>
  (item: TableItem, column: TableColumn): ReactNode => {
    const survey = item as Survey

    if (column.type === "operation") {
      const operation = getOperation(survey, 0)
      if (!operation) return undefined

      return t(`operations:${operation.operation}`)
    }
    if (column.type === "voted-approval") {
      return getVotedApproval(t, survey, policies)
    }

    if (column.accessor === "totalVotes") {
      return survey.isVariableApproval ? survey.totalVotes : ""
    }
    if (column.accessor === "votesRequiredToWin") {
      return survey.isVariableApproval ? survey.votesRequiredToWin : ""
    }
    if (column.type === "win-percentage") {
      return survey.lastWin !== -1 && survey.isVariableApproval
        ? formatPercents(survey.options[survey.lastWin].votePercents)
        : ""
    }
  }
