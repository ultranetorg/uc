import { TFunction } from "i18next"

import { Proposal } from "types"
import { formatNabb, formatNabbShort } from "utils"

export const renderNabb = (t: TFunction, proposal: Proposal) => {
  const title = formatNabb(t, proposal.neither.length, proposal.any.length, proposal.ban.length, proposal.banish.length)
  const value = formatNabbShort(
    proposal.neither.length,
    proposal.any.length,
    proposal.ban.length,
    proposal.banish.length,
  )
  return (
    <div className="truncate" title={title}>
      {value}
    </div>
  )
}
