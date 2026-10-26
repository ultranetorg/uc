import { ReactNode } from "react"
import { TFunction } from "i18next"

import { OperationType, ReviewProposal } from "types"
import { renderUser, renderVotes } from "ui/renderers2"
import { ButtonOutline, ButtonPrimary, TableColumn, TableItem } from "ui/components"
import { isUserVoted } from "utils"
import { renderActions, renderCommon, renderPublication, renderText } from "ui/renderers/utils"

const renderDeleteActions = (
  t: TFunction,
  onApprove: () => void,
  onReject: () => void,
  loadingAction?: "approve" | "reject",
  locked?: boolean,
) => (
  <div className="flex justify-end gap-5">
    <ButtonPrimary
      className="h-9 w-20 capitalize"
      label={t("common:delete")}
      loading={loadingAction === "approve"}
      disabled={loadingAction === "reject" || locked}
      onClick={e => {
        onApprove()
        e.stopPropagation()
        e.preventDefault()
      }}
    />
    <ButtonOutline
      className="h-9 w-20 capitalize"
      label={t("common:cancel")}
      loading={loadingAction === "reject"}
      disabled={loadingAction === "approve" || locked}
      onClick={e => {
        onReject()
        e.stopPropagation()
        e.preventDefault()
      }}
    />
  </div>
)

export const getReviewsPageItemRenderer =
  (
    t: TFunction,
    onApprove: (id: string) => void,
    onReject: (id: string) => void,
    votesRequired: Partial<Record<OperationType, number>>,
    loadingItem?: { id: string; action: "approve" | "reject" } | undefined,
    currentUserId?: string,
    editUserId?: string,
  ) =>
  (item: TableItem, column: TableColumn): ReactNode => {
    const proposal = item as ReviewProposal
    const voterId = proposal.operation === "review-edit" ? editUserId : currentUserId
    const isButtonsDisabled = !voterId || isUserVoted(voterId, proposal)

    switch (column.type) {
      case "account":
        return renderUser(proposal.by)

      case "publication":
        return proposal.publication !== null ? renderPublication(proposal.publication) : "Publication removed"

      case "text":
        return renderText(proposal.reviewText)

      case "review-action":
        return renderActions(
          t,
          () => onApprove(proposal.id),
          () => onReject(proposal.id),
          loadingItem?.id === item.id ? loadingItem.action : undefined,
          isButtonsDisabled || (loadingItem && loadingItem.id !== item.id),
        )

      case "delete-actions":
        return renderDeleteActions(
          t,
          () => onApprove(proposal.id),
          () => onReject(proposal.id),
          loadingItem?.id === item.id ? loadingItem.action : undefined,
          isButtonsDisabled || (loadingItem && loadingItem.id !== item.id),
        )

      case "votes":
        return renderVotes(
          proposal.yes.map(x => x.length),
          votesRequired[proposal.operation] ?? 0,
        )
    }

    return renderCommon(t, column, proposal)
  }
