import { TFunction } from "i18next"

import { Proposal } from "types"

export const getOptionTitle = (t: TFunction, proposal: Proposal, index: number): string =>
  proposal.yes.length === 1
    ? t("common:yes")
    : proposal.optionTitles?.[index] || t("common:optionNumber", { number: index + 1 })
