import { useCallback, useState } from "react"
import { useTranslation } from "react-i18next"

import { SvgArrowLeft } from "assets"
import { Proposal, ProposalComment, TotalItemsResult } from "types"
import { Breadcrumbs, BreadcrumbsItemProps, ButtonOutline, ButtonPrimary } from "ui/components"
import { AlternativeOptions, CommentsSection, OptionsCollapsesList, ProposalInfo } from "ui/components/proposal"
import { useParams } from "react-router-dom"

type PageState = "voting" | "results"

export type ProposalViewProps = {
  parentBreadcrumb?: BreadcrumbsItemProps
  isFetching: boolean
  proposal?: Proposal
  isCommentsFetching: boolean
  comments?: TotalItemsResult<ProposalComment>
}

export const ProposalView = ({ parentBreadcrumb, proposal, isCommentsFetching, comments }: ProposalViewProps) => {
  const { siteId } = useParams()
  const { t } = useTranslation("proposal")

  const [pageState, setPageState] = useState<PageState>("voting")

  const togglePageState = useCallback(() => setPageState(prev => (prev === "voting" ? "results" : "voting")), [])

  if (!proposal || !comments) {
    return <>LOADING</>
  }

  return (
    <div className="flex flex-col gap-6">
      <div className="flex flex-col gap-2">
        <Breadcrumbs
          fullPath={true}
          items={[
            { path: `/${siteId}`, title: t("home") },
            ...(parentBreadcrumb ? [parentBreadcrumb] : []),
            { title: proposal.title || proposal.id },
          ]}
        />
        <div className="flex flex-col gap-4">
          <span className="text-3.5xl font-semibold leading-10">{proposal.title}</span>
          {proposal.text && <span className="text-2sm leading-5">{proposal.text}</span>}
        </div>
      </div>
      <div className="flex gap-8">
        <div className="flex flex-col gap-8">
          <OptionsCollapsesList
            className="max-w-187.5"
            showResults={pageState == "results"}
            votesText={t("common:votes")}
          />
          <AlternativeOptions />
          <hr className="h-px border-0 bg-gray-300" />
          <CommentsSection isFetching={isCommentsFetching} comments={comments} />
        </div>
        <div className="flex flex-col gap-6">
          <ProposalInfo
            className="w-87.5"
            createdBy={proposal?.byAccount}
            createdAt={proposal?.creationTime}
            daysLeft={7}
          />
          {pageState == "voting" ? (
            <ButtonOutline className="h-11 w-full" label="Show results" onClick={togglePageState} />
          ) : (
            <ButtonPrimary
              label="Back to options"
              onClick={togglePageState}
              iconBefore={<SvgArrowLeft className="fill-white" />}
            />
          )}
        </div>
      </div>
    </div>
  )
}
