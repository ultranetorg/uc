import { ReactNode } from "react"
import { TFunction } from "i18next"

import { PerpetualSurvey, SiteApprovalPolicyChange } from "types"
import { TableColumn, TableItem } from "ui/components/Table"
import { formatPercents } from "utils"

const getOperation = (survey: PerpetualSurvey, index: number): SiteApprovalPolicyChange | undefined =>
  survey?.options && survey.options.length > 0 && index < survey.options.length
    ? (survey.options[index].operation as SiteApprovalPolicyChange)
    : undefined

export const perpetualSurveysItemRenderer =
  (t: TFunction) =>
  (item: TableItem, column: TableColumn): ReactNode => {
    const survey = item as PerpetualSurvey

    switch (column.type) {
      case "operation": {
        const operation = getOperation(survey, 0)
        if (!operation) return undefined

        return t(`operations:${operation.operation}`)
      }

      case "voted-approval": {
        if (survey.lastWin === -1) return "-"

        const winOperation = getOperation(survey, survey.lastWin)!
        return t(`approvalRequirement:${winOperation.approval}`)
      }

      case "win-percentage":
        return survey.lastWin !== -1 ? formatPercents(survey.options[survey.lastWin].votePercents) : ""
    }
  }
