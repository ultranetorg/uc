import { TFunction } from "i18next"

import { Proposal } from "types"
import { getLeadingOptionIndexes, getLeadingVotes } from "utils"

import { getOptionTitle } from "./getOptionTitle"

export const renderLeadingOption = (t: TFunction, proposal: Proposal) => {
  const leaders = getLeadingOptionIndexes(proposal)
  const text = leaders.map(x => getOptionTitle(t, proposal, x)).join(", ")
  const specials = [
    { key: "common:any", count: proposal.any.length },
    { key: "common:neither", count: proposal.neither.length },
    { key: "common:ban", count: proposal.ban.length },
    { key: "common:banish", count: proposal.banish.length },
  ].filter(x => x.count > 0)

  return (
    <div className="flex flex-col gap-1">
      {leaders.length > 0 ? (
        <>
          <span className="truncate text-2sm leading-5" title={text}>
            {text}
          </span>
          <span className="text-2xs leading-4 text-gray-500">
            {getLeadingVotes(proposal)} {t("common:votes")}
          </span>
        </>
      ) : (
        <span className="text-gray-500">{t("common:noVotesYet")}</span>
      )}
      {specials.length > 0 && (
        <div className="flex flex-wrap gap-1">
          {specials.map(x => (
            <span key={x.key} className="rounded-full border border-gray-300 bg-gray-100 px-2 text-2xs leading-4">
              {t(x.key)} {x.count}
            </span>
          ))}
        </div>
      )}
    </div>
  )
}
