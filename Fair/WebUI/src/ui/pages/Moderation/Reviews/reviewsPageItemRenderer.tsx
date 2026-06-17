import { ReactNode } from "react"
import { TFunction } from "i18next"

import { ReviewProposal } from "types"
import { renderUser, renderVotes } from "ui/renderers2"
import { TableColumn, TableItem } from "ui/components"
import { isUserVoted } from "utils"
import { renderActions, renderCommon, renderPublication, renderText } from "ui/renderers/utils"

export const getReviewsPageItemRenderer =
  (
    t: TFunction,
    onApprove: (id: string) => void,
    onReject: (id: string) => void,
    votesRequired: { create: number; edit: number },
    loadingItem?: { id: string; action: "approve" | "reject" } | undefined,
    currentUserId?: string,
  ) =>
  (item: TableItem, column: TableColumn): ReactNode => {
    const proposal = item as ReviewProposal
    const isVoted = isUserVoted(currentUserId, proposal)

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
          isVoted || (loadingItem && loadingItem.id !== item.id),
        )

      case "votes":
        return proposal.operation === "review-creation"
          ? renderVotes(
              proposal.yes.map(x => x.length),
              votesRequired.create,
            )
          : renderVotes(
              proposal.yes.map(x => x.length),
              votesRequired.edit,
            )
    }

    return renderCommon(t, column, proposal)
  }
