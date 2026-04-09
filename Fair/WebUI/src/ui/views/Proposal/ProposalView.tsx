import { ComponentType, memo, useCallback, useEffect, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"
import { Link, useNavigate, useParams } from "react-router-dom"

import { useModerationContext } from "app"
import { SvgArrowLeft, SvgEyeSm } from "assets"
import { useGetModeratorDiscussionComments } from "entities"
import { useTransactMutationWithStatus } from "entities/node"
import { ProposalCommentCreation, ProposalDetails, ProposalVoting, SpecialChoice } from "types"
import { BreadcrumbsItemProps, ButtonBar, ButtonOutline, ButtonPrimary, Separator } from "ui/components"
import { AlternativeOptions, CommentsSection, ProposalInfo } from "ui/components/proposal"
import { ModerationHeader } from "ui/components/specific"
import { getVotedIndex, isVoted, showToast } from "utils"

import {
  DefaultContent,
  ProposalViewContentProps,
  PublicationCreationContent,
  PublicationDeletionContent,
  PublicationUpdationContent,
  VoteAction,
  VoteStatus,
} from "./content"
import { PageState } from "./types"
import { getProductId, getPublicationId } from "./utils"

const renderByOperationType: Record<string, ComponentType<ProposalViewContentProps>> = {
  "publication-creation": PublicationCreationContent,
  "publication-updation": PublicationUpdationContent,
  "publication-deletion": PublicationDeletionContent,
}

// Set voted value in order to disable all buttons inside OptionsCollapsesList.
const ALREADY_VOTED = 100

export type ProposalViewProps = {
  parentBreadcrumb?: BreadcrumbsItemProps
  isFetching: boolean
  proposal?: ProposalDetails
  previousPath?: string
}

export const ProposalView = memo(({ parentBreadcrumb, proposal, previousPath }: ProposalViewProps) => {
  const { siteId } = useParams()
  const { getOperationVoterId } = useModerationContext()
  const navigate = useNavigate()
  const { mutate } = useTransactMutationWithStatus()
  const { t } = useTranslation("proposalView")

  const voterId = getOperationVoterId(proposal?.operation)

  const [voteStatus, setVoteStatus] = useState<VoteStatus>("idle")
  const [pageState, setPageState] = useState<PageState>("voting")
  const [votedValue, setVotedValue] = useState<number | undefined>()
  const [voteAction, setVoteAction] = useState<VoteAction | undefined>()
  const [commentSubmitting, setCommentSubmitting] = useState(false)

  const {
    isFetching: isCommentsFetching,
    data: comments,
    refetch: refetchComments,
  } = useGetModeratorDiscussionComments(siteId, proposal?.id)

  const NestedContent = proposal?.operation ? renderByOperationType[proposal.operation] : undefined
  const isPublicationMode = !!NestedContent

  const productId = useMemo(
    () => (isPublicationMode ? getProductId(proposal) : undefined),
    [isPublicationMode, proposal],
  )
  const publicationId = useMemo(
    () => (isPublicationMode ? getPublicationId(proposal) : undefined),
    [isPublicationMode, proposal],
  )

  const parentBreadcrumbs = useMemo(
    () =>
      parentBreadcrumb
        ? [{ path: `/${siteId}/m`, title: t("common:proposals") }, parentBreadcrumb]
        : { path: `/${siteId}/m`, title: t("common:proposals") },
    [parentBreadcrumb, siteId, t],
  )

  const togglePageState = useCallback(() => setPageState(prev => (prev === "voting" ? "results" : "voting")), [])

  const handleVoteClick = useCallback(
    (value: number) => {
      setVotedValue(value)
      setVoteStatus("voting")
      const operation = new ProposalVoting(proposal!.id, voterId!, value)
      mutate(operation, {
        onSuccess: () => {
          showToast(t("toast:voted"), "success")
          setVoteStatus("voted")
          navigate(previousPath ?? `/${siteId}/m`)
        },
        onError: err => {
          showToast(err.toString(), "error")
          setVoteStatus("idle")
        },
        onSettled: () => setVotedValue(undefined),
      })
    },
    [mutate, navigate, previousPath, proposal, siteId, t, voterId],
  )

  const vote = useCallback(
    (action: VoteAction) => {
      setVoteStatus("voting")
      setVoteAction(action)
      const operation = new ProposalVoting(proposal!.id, voterId!, action === "approve" ? 0 : SpecialChoice.Neither)
      mutate(operation, {
        onSuccess: () => {
          showToast(t("toast:publicationVoted"), "success")
          navigate(`/${siteId}/m/c/p`)
        },
        onError: err => {
          showToast(err.toString(), "error")
          setVoteStatus("idle")
          setVoteAction(undefined)
        },
      })
    },
    [mutate, navigate, proposal, siteId, t, voterId],
  )

  const handleApprove = useCallback(() => vote("approve"), [vote])
  const handleReject = useCallback(() => vote("reject"), [vote])

  const handleCommentSubmit = useCallback(
    (comment: string) => {
      setCommentSubmitting(true)
      if (!isPublicationMode) setVotedValue(ALREADY_VOTED)
      const operation = new ProposalCommentCreation(proposal!.id, voterId!, comment)
      mutate(operation, {
        onSuccess: () => {
          refetchComments()
          showToast(t("toast:commentAdded"), "success")
        },
        onError: err => showToast(err.toString(), "error"),
        onSettled: () => {
          setCommentSubmitting(false)
          if (!isPublicationMode) setVotedValue(undefined)
        },
      })
    },
    [isPublicationMode, mutate, proposal, refetchComments, t, voterId],
  )

  useEffect(() => {
    if (isPublicationMode) {
      if (isVoted(voterId, proposal)) {
        setVoteStatus("voted")
      }
    } else {
      const votedIndex = getVotedIndex(voterId, proposal)
      if (votedIndex !== undefined) {
        setVoteStatus("voted")
        setVotedValue(votedIndex)
      }
    }
  }, [isPublicationMode, proposal, voterId])

  if (!proposal || !comments) {
    return <>LOADING</>
  }

  return (
    <div className="flex flex-col gap-6">
      <ModerationHeader
        title={proposal.title ?? proposal.id}
        parentBreadcrumbs={parentBreadcrumbs}
        components={
          isPublicationMode && !!voterId && voteStatus !== "voted" ? (
            <ButtonBar className="items-center">
              <ButtonPrimary
                className="h-11 w-43.75"
                label="Suggest to approve"
                onClick={handleApprove}
                disabled={voteStatus === "voting"}
                loading={voteAction === "approve"}
              />
              <ButtonOutline
                className="h-11 w-43.75"
                label="Suggest to reject"
                onClick={handleReject}
                disabled={voteStatus === "voting"}
                loading={voteAction === "reject"}
              />
              <Separator className="h-8" />
              <Link
                to={`/${siteId}/m/v`}
                state={{
                  productId,
                  publicationId,
                  proposalId: proposal.id,
                  previousPath: `/${siteId}/m/c/p/${proposal.id}`,
                }}
              >
                <ButtonOutline
                  disabled={voteStatus === "voting"}
                  className="h-11 w-40 capitalize"
                  label={t("common:preview")}
                  iconBefore={<SvgEyeSm className="fill-gray-800" />}
                />
              </Link>
            </ButtonBar>
          ) : undefined
        }
      />
      {NestedContent && <NestedContent t={t} proposal={proposal} voteStatus={voteStatus} />}
      <div className="flex gap-8">
        <div className="flex w-full flex-col gap-8">
          {!isPublicationMode && (
            <DefaultContent
              t={t}
              proposal={proposal}
              pageState={pageState}
              voteStatus={voteStatus}
              votedValue={votedValue}
              onVoteClick={handleVoteClick}
            />
          )}
          {!isPublicationMode && !!voterId && (
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
          <ProposalInfo className="w-87.5" createdBy={proposal.by} createdAt={proposal.creationTime} daysLeft={7} />
          {!isPublicationMode &&
            (pageState === "voting" ? (
              <ButtonOutline className="h-11 w-full" label={t("showResults")} onClick={togglePageState} />
            ) : (
              <ButtonPrimary
                label="Back to options"
                onClick={togglePageState}
                iconBefore={<SvgArrowLeft className="fill-white" />}
              />
            ))}
        </div>
      </div>
    </div>
  )
})
