import { ComponentType, memo, useCallback, useEffect, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"
import { Link, Navigate, useNavigate, useParams } from "react-router-dom"

import { useModerationContext } from "app"
import { SvgEyeSm } from "assets"
import { useGetModeratorDiscussionComments } from "entities"
import { useTransactMutationWithStatus } from "entities/node"
import { ProposalCommentCreation, ProposalDetails, ProposalVoting, SpecialChoice } from "types"
import { BreadcrumbsItemProps, ButtonBar, ButtonOutline, ButtonPrimary, Separator } from "ui/components"
import { CommentsSection, ProposalInfo } from "ui/components/proposal"
import { ModerationHeader } from "ui/components/specific"
import { isVoted, showToast } from "utils"

import { PublicationOwnerProvider } from "./providers/publicationOwner"
import {
  ProposalCompareFieldsView,
  PublicationCreationProposalView,
  ProposalTypeViewProps,
  VoteStatus,
  VoteAction,
} from "./views"
import { getProductId, getPublicationId } from "./utils"

const renderByOperationType: Record<string, ComponentType<ProposalTypeViewProps>> = {
  "publication-creation": PublicationCreationProposalView,
  "publication-updation": ProposalCompareFieldsView,
  "publication-deletion": PublicationCreationProposalView,
}

export type PublicationProposalViewProps = {
  parentBreadcrumb?: BreadcrumbsItemProps
  isFetching: boolean
  proposal?: ProposalDetails
}

export const PublicationProposalView = memo(({ parentBreadcrumb, proposal }: PublicationProposalViewProps) => {
  const { siteId } = useParams()
  const { getOperationVoterId } = useModerationContext()
  const { mutate } = useTransactMutationWithStatus()
  const navigate = useNavigate()
  const { t } = useTranslation("proposalView")

  const voterId = getOperationVoterId(proposal?.operation)

  const [voteStatus, setVoteStatus] = useState<VoteStatus>("idle")
  const [voteAction, setVoteAction] = useState<VoteAction | undefined>()
  const [commentSubmitting, setCommentSubmitting] = useState(false)

  const {
    isFetching: isCommentsFetching,
    data: comments,
    refetch: refetchComments,
  } = useGetModeratorDiscussionComments(siteId, proposal?.id)

  const productId = useMemo(() => getProductId(proposal), [proposal])
  const publicationId = useMemo(() => getPublicationId(proposal), [proposal])

  const NestedView = proposal?.operation ? renderByOperationType[proposal.operation] : undefined

  const parentBreadcrumbs = useMemo(
    () =>
      parentBreadcrumb
        ? [{ path: `/${siteId}/m`, title: t("common:proposals") }, parentBreadcrumb]
        : { path: `/${siteId}/m`, title: t("common:proposals") },
    [parentBreadcrumb, siteId, t],
  )

  const vote = useCallback(
    (voteAction: VoteAction) => {
      setVoteStatus("voting")
      setVoteAction(voteAction)
      const operation = new ProposalVoting(proposal!.id, voterId!, voteAction === "approve" ? 0 : SpecialChoice.Neither)
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
      const operation = new ProposalCommentCreation(proposal!.id, voterId!, comment)
      mutate(operation, {
        onSuccess: () => {
          refetchComments()
          showToast(t("toast:commentAdded"), "success")
        },
        onError: err => showToast(err.toString(), "error"),
        onSettled: () => {
          setCommentSubmitting(false)
        },
      })
    },
    [mutate, proposal, refetchComments, t, voterId],
  )

  useEffect(() => {
    if (isVoted(voterId, proposal)) {
      setVoteStatus("voted")
    }
  }, [proposal, voterId])

  if (!proposal || !comments) {
    return <>LOADING</>
  }

  if (!NestedView) {
    return <Navigate to={`/${siteId}`} replace />
  }

  return (
    <div className="flex flex-col gap-6">
      <ModerationHeader
        title={proposal.title ?? proposal.id}
        parentBreadcrumbs={parentBreadcrumbs}
        components={
          <>
            {!!voterId && voteStatus !== "voted" && (
              <ButtonBar className="items-center">
                <ButtonPrimary
                  className="h-11 w-43.75"
                  label="Suggest to approve"
                  onClick={handleApprove}
                  disabled={voteStatus === "voting"}
                  loading={voteAction === "approve"}
                />
                <ButtonOutline
                  className="h-11 w-40"
                  label="Suggest to reject"
                  onClick={handleReject}
                  disabled={voteStatus === "voting"}
                  loading={voteAction === "reject"}
                />
                <Separator className="h-8" />
                <Link
                  to={`/${siteId}/m/v`}
                  state={{
                    productId: productId,
                    publicationId: publicationId,
                    proposalId: proposal?.id,
                    previousPath: `/${siteId}/m/c/p/${proposal?.id}`,
                  }}
                >
                  <ButtonOutline
                    disabled={voteStatus === "voting"}
                    className="h-11 w-52"
                    label="Preview publication"
                    iconBefore={<SvgEyeSm className="fill-gray-800" />}
                  />
                </Link>
              </ButtonBar>
            )}
          </>
        }
      />
      <PublicationOwnerProvider owner={proposal?.by}>
        {NestedView && <NestedView t={t} proposal={proposal} voteStatus={voteStatus} />}
        <div className="flex gap-8">
          <div className="flex w-full flex-col gap-8">
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
          </div>
        </div>
      </PublicationOwnerProvider>
    </div>
  )
})
