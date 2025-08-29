import { useCallback, useState } from "react"
import { TFunction } from "i18next"

import { Proposal } from "types"
import { ButtonOutline, ButtonPrimary } from "ui/components"
import { AlternativeOptions, OptionsCollapsesList, ProposalInfo } from "ui/components/proposal"

type PageState = "voting" | "results"

export type ProposalViewProps = {
  t: TFunction
  proposal: Proposal
}

export const ProposalView = ({ t, proposal }: ProposalViewProps) => {
  const [pageState, setPageState] = useState<PageState>("voting")

  const togglePageState = useCallback(() => setPageState(prev => (prev === "voting" ? "results" : "voting")), [])

  return (
    <div className="flex flex-col gap-6">
      <span className="text-3.5xl font-semibold leading-10">{proposal.title}</span>
      <div className="flex gap-8">
        <div className="flex flex-col gap-8">
          <OptionsCollapsesList className="max-w-187.5" />
          <AlternativeOptions />
          <hr className="h-px border-0 bg-gray-300" />
        </div>
        <div className="flex flex-col gap-6">
          <ProposalInfo
            className="w-87.5"
            createdBy={proposal.byAccount}
            createdAt={proposal.creationTime}
            daysLeft={7}
          />
          {pageState == "voting" ? (
            <ButtonOutline className="h-11 w-full" label="Show results" onClick={togglePageState} />
          ) : (
            <ButtonPrimary label="Back to options" onClick={togglePageState} />
          )}
        </div>
      </div>
    </div>
  )
}
