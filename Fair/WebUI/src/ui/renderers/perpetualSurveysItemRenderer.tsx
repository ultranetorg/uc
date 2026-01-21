import { ReactNode } from "react"
import { TFunction } from "i18next"

import { PerpetualSurvey, SiteApprovalPolicyChange } from "types"
import { TableColumn, TableItem } from "ui/components/Table"

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

      case "votedApproval": {
        const winOperation = survey.lastWin !== -1 ? getOperation(survey, survey.lastWin) : undefined
        return winOperation ? t(`approvalRequirement:${winOperation.approval}`) : t("noVotes")
      }
    }
  }
