import { TFunction } from "i18next"

import { Proposal } from "types"

import { PageState } from "../types"

export type ProposalTypeViewProps = {
  t: TFunction
  pageState: PageState
  proposal: Proposal
}
