import { useCallback, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"

import { SvgArrowLeft } from "assets"
import { Proposal, ProposalComment, PublicationCreation, TotalItemsResult } from "types"
import { Breadcrumbs, BreadcrumbsItemProps, ButtonOutline, ButtonPrimary } from "ui/components"
import { AlternativeOptions, CommentsSection, ProposalInfo } from "ui/components/proposal"
import { useParams } from "react-router-dom"
import { ProductFields } from "../components/proposal/ProductsFields"

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

  const productIds = useMemo(
    () =>
      proposal?.options
        ?.map(option => option.operation)
        .filter((operation): operation is PublicationCreation => operation.$type === "publication-creation")
        .map(operation => operation.productId),
    [proposal],
  )

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
        </div>
      </div>
      {productIds?.length ? (
        <div className="grid grid-cols-[auto_200px] gap-6">
          <div>
            <ProductFields productIds={productIds} />
          </div>
          <div>
            <ProposalInfo createdBy={proposal?.byAccount} createdAt={proposal?.creationTime} daysLeft={7} />
          </div>
        </div>
      ) : (
        ""
      )}
      <div className="flex gap-8">
        <div className="flex flex-col gap-8">
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
