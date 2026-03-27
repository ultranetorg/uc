import { memo, useCallback, useEffect, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"
import { useNavigate, useParams } from "react-router-dom"

import { useModerationContext } from "app"
import { SvgArrowLeft } from "assets"
import { useGetModeratorDiscussionComments } from "entities"
import { useTransactMutationWithStatus } from "entities/node"
import { ProposalCommentCreation, ProposalDetails, ProposalVoting } from "types"
import { BreadcrumbsItemProps, ButtonOutline, ButtonPrimary } from "ui/components"
import { AlternativeOptions, CommentsSection, ProposalInfo } from "ui/components/proposal"
import { ModerationHeader } from "ui/components/specific"
import { getVotedIndex, showToast } from "utils"

import { PublicationOwnerProvider } from "./providers/publicationOwner"
import { PageState } from "./types"
import { ProposalDefaultView, VoteStatus } from "./views"

export type ProposalViewProps = {
  parentBreadcrumb?: BreadcrumbsItemProps
  isFetching: boolean
  proposal?: ProposalDetails
}

// Set voted value in order to disable all buttons inside OptionsCollapsesList.
const ALREADY_VOTED = 100

export const ProposalView = memo(({ parentBreadcrumb, proposal }: ProposalViewProps) => {
  const { siteId } = useParams()
  const { getOperationVoterId } = useModerationContext()
  const navigate = useNavigate()
  const { mutate } = useTransactMutationWithStatus()
  const { t } = useTranslation("proposalView")

  const voterId = getOperationVoterId(proposal?.operation)

  const [voteStatus, setVoteStatus] = useState<VoteStatus>("idle")
  const [pageState, setPageState] = useState<PageState>("voting")
  const [votedValue, setVotedValue] = useState<number | undefined>()
  const [commentSubmitting, setCommentSubmitting] = useState(false)

  const {
    isFetching: isCommentsFetching,
    data: comments,
    refetch: refetchComments,
  } = useGetModeratorDiscussionComments(siteId, proposal?.id)

  const togglePageState = useCallback(() => setPageState(prev => (prev === "voting" ? "results" : "voting")), [])

  const parentBreadcrumbs = useMemo(
    () =>
      parentBreadcrumb
        ? [{ path: `/${siteId}/m`, title: t("common:proposals") }, parentBreadcrumb]
        : { path: `/${siteId}/m`, title: t("common:proposals") },
    [parentBreadcrumb, siteId, t],
  )

  const handleVoteClick = useCallback(
    (value: number) => {
      setVotedValue(value)
      setVoteStatus("voting")
      const operation = new ProposalVoting(proposal!.id, voterId!, value)
      mutate(operation, {
        onSuccess: () => {
          showToast(t("toast:voted"), "success")
          navigate(`/${siteId}/m`)
          setVoteStatus("voted")
        },
        onError: err => {
          showToast(err.toString(), "error")
          setVoteStatus("idle")
        },
        onSettled: () => setVotedValue(undefined),
      })
    },
    [mutate, navigate, proposal, siteId, t, voterId],
  )

  const handleCommentSubmit = useCallback(
    (comment: string) => {
      setCommentSubmitting(true)
      setVotedValue(ALREADY_VOTED)
      const operation = new ProposalCommentCreation(proposal!.id, voterId!, comment)
      mutate(operation, {
        onSuccess: () => {
          refetchComments()
          showToast(t("toast:commentAdded"), "success")
        },
        onError: err => showToast(err.toString(), "error"),
        onSettled: () => {
          setCommentSubmitting(false)
          setVotedValue(undefined)
        },
      })
    },
    [mutate, proposal, refetchComments, t, voterId],
  )

  useEffect(() => {
    const votedIndex = getVotedIndex(voterId, proposal)
    if (votedIndex !== undefined) {
      setVoteStatus("voted")
      setVotedValue(votedIndex)
    }
  }, [proposal, voterId])

  if (!proposal || !comments) {
    return <>LOADING</>
  }

  return (
    <div className="flex flex-col gap-6">
      <ModerationHeader title={proposal.title ?? proposal.id} parentBreadcrumbs={parentBreadcrumbs} />
      <PublicationOwnerProvider owner={proposal?.by}>
        <div className="flex gap-8">
          <div className="flex w-full flex-col gap-8">
            <ProposalDefaultView
              t={t}
              proposal={proposal}
              pageState={pageState}
              voteStatus={voteStatus}
              votedValue={votedValue}
              onVoteClick={handleVoteClick}
            />
            {voterId && (
              <AlternativeOptions
                hideVoteButton={voteStatus === "voted"}
                votedValue={votedValue}
                onVoteClick={handleVoteClick}
              />
            )}
            <hr className="h-px border-0 bg-gray-300" />
            <CommentsSection
              inputDisabled={voteStatus === "voting" || commentSubmitting}
              inputLoading={commentSubmitting}
              showCommentInput={!!voterId}
              isFetching={isCommentsFetching}
              comments={comments}
              onCommentSubmit={handleCommentSubmit}
            />
          </div>
          <div className="flex flex-col gap-6">
            <ProposalInfo className="w-87.5" createdBy={proposal?.by} createdAt={proposal?.creationTime} daysLeft={7} />
            {pageState === "voting" ? (
              <ButtonOutline className="h-11 w-full" label={t("showResults")} onClick={togglePageState} />
            ) : (
              <ButtonPrimary
                label="Back to options"
                onClick={togglePageState}
                iconBefore={<SvgArrowLeft className="fill-white" />}
              />
            )}
          </div>
        </div>
      </PublicationOwnerProvider>
    </div>
  )
})
