import { ReactNode } from "react"
import { TFunction } from "i18next"

import { PerpetualSurvey, Policy, SiteApprovalPolicyChange } from "types"
import { TableColumn, TableItem } from "ui/components/Table"
import { formatPercents } from "utils"

const getOperation = (survey: PerpetualSurvey, index: number): SiteApprovalPolicyChange | undefined =>
  survey?.options && survey.options.length > 0 && index < survey.options.length
    ? (survey.options[index].operation as SiteApprovalPolicyChange)
    : undefined

const getVotedApproval = (t: TFunction, survey: PerpetualSurvey, policies?: Policy[]): string => {
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

export const perpetualSurveysItemRenderer =
  (t: TFunction, policies?: Policy[]) =>
  (item: TableItem, column: TableColumn): ReactNode => {
    const survey = item as PerpetualSurvey

    switch (column.type) {
      case "operation": {
        const operation = getOperation(survey, 0)
        if (!operation) return undefined

        return t(`operations:${operation.operation}`)
      }

      case "voted-approval": {
        return getVotedApproval(t, survey, policies)
      }

      case "win-percentage":
        return survey.lastWin !== -1 ? formatPercents(survey.options[survey.lastWin].votePercents) : ""
    }
  }
