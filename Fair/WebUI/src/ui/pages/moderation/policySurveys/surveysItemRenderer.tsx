import { ReactNode } from "react"
import { TFunction } from "i18next"

import { SvgInfoCircle } from "assets"
import { Survey, Policy } from "types"
import { Tooltip, TableColumn, TableItem } from "ui/components"

import {
  CELL_CLASSNAME,
  getOperation,
  renderLeadingOption,
  renderProgress,
  renderVotedApproval,
  renderYourVote,
  stopPropagation,
} from "./utils"

export const surveysItemRenderer =
  (t: TFunction, policies?: Policy[], publisherIds?: string[]) =>
  (item: TableItem, column: TableColumn): ReactNode => {
    const survey = item as Survey

    if (column.type === "operation") {
      const operation = getOperation(survey, 0)
      if (!operation) return undefined

      return (
        <div className={CELL_CLASSNAME}>
          {t(`operations:${operation.operation}`)}{" "}
          <Tooltip text={t(operation.operation)}>
            <SvgInfoCircle className="cursor-default stroke-gray-800" onClick={stopPropagation} />
          </Tooltip>
        </div>
      )
    }
    if (column.type === "voted-approval") {
      return renderVotedApproval(t, survey, policies)
    }

    if (!survey.isVariableApproval) return "-"

    if (column.type === "leading-option") {
      return renderLeadingOption(t, survey, policies)
    }
    if (column.type === "progress") {
      return renderProgress(survey)
    }
    if (column.type === "your-vote") {
      return renderYourVote(t, survey, publisherIds)
    }
  }
