import { TFunction } from "i18next"

import { Proposal, SpecialChoice } from "types"
import { Badge } from "ui/components"
import { getVotedChoice } from "utils"

import { getOptionTitle } from "./getOptionTitle"

const SPECIAL_CHOICE_KEYS: Record<number, string> = {
  [SpecialChoice.Neither]: "common:neither",
  [SpecialChoice.Any]: "common:any",
  [SpecialChoice.Ban]: "common:ban",
  [SpecialChoice.Banish]: "common:banish",
}

export const renderYourVote = (t: TFunction, proposal: Proposal, voterIds: string[]) => {
  if (voterIds.length === 0) return "-"

  const choice = getVotedChoice(voterIds, proposal)
  if (choice === undefined) {
    return <Badge variant="warning">{t("common:notVoted")}</Badge>
  }

  const text = choice >= 0 ? getOptionTitle(t, proposal, choice) : t(SPECIAL_CHOICE_KEYS[choice])
  return (
    <div className="truncate" title={text}>
      {text}
    </div>
  )
}
