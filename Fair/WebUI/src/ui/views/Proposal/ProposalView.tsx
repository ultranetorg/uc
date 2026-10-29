import { ComponentType, memo, useCallback, useEffect, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"
import { Link, useNavigate } from "react-router-dom"

import { useSignInContext, useStoreContext, useStorePoliciesContext, useStoreRolesContext, useUserContext } from "app"
import { SvgArrowLeft, SvgEyeSm } from "assets"
import { useGetProposalComments, useInvalidation } from "entities"
import { useTransactMutationWithStatus } from "entities/iccpNode"
import { useApprovalId, useApprovalIdentityForOperation, useResolveStoreId } from "hooks"
import { OperationType, ProposalCommentCreation, ProposalDetails, ProposalVoting, SpecialChoice } from "types"
import { BreadcrumbsItemProps, ButtonBar, ButtonOutline, ButtonPrimary, Separator } from "ui/components"
import { AlternativeOptions, CommentsSection, ProposalInfo } from "ui/components/proposal"
import { ModerationHeader, ModerationHeaderProps } from "ui/components/specific"
import { getVoterIds, getVotedIndex, isReferendum as isReferendum_, isVoted, routes, showToast } from "utils"

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
import { useCanCreateComment } from "./useCanCreateComment"
import { getProductId, getPublicationId } from "./utils"

const renderByOperationType: Partial<Record<OperationType, ComponentType<ProposalViewContentProps>>> = {
  "publication-creation": PublicationCreationContent,
  "publication-updation": PublicationUpdationContent,
  "publication-deletion": PublicationDeletionContent,
}

// Set voted value in order to disable all buttons inside OptionsCollapsesList.
const ALREADY_VOTED = 100

export interface ProposalViewProps {
  parentBreadcrumbs?: BreadcrumbsItemProps | BreadcrumbsItemProps[]
  isFetching: boolean
  proposal?: ProposalDetails
  previousPath?: string
  headerProps?: Partial<ModerationHeaderProps>
}

export const ProposalView = memo(({ parentBreadcrumbs, proposal, previousPath, headerProps }: ProposalViewProps) => {
  const navigate = useNavigate()
  const { t } = useTranslation("proposalView")

  const approvalId = useApprovalId()
  const canCreateComment = useCanCreateComment(proposal?.operation)
  const { invalidateOperation, invalidateProposal } = useInvalidation()
  const approval = useApprovalIdentityForOperation(proposal?.operation)
  const storeId = useResolveStoreId()
  const { startSignIn } = useSignInContext()
  const { store } = useStoreContext()
  const policies = useStorePoliciesContext()
  const { isModerator, publisherIds } = useStoreRolesContext()
  const { user } = useUserContext()
  const { mutate } = useTransactMutationWithStatus()

  const isReferendum = isReferendum_(proposal?.operation, policies)

  const voterIds = useMemo(
    () => (proposal ? getVoterIds(proposal.operation, policies, isModerator ? user?.id : undefined, publisherIds) : []),
    [proposal, policies, isModerator, user?.id, publisherIds],
  )

  const [voteStatus, setVoteStatus] = useState<VoteStatus>("idle")
  const [pageState, setPageState] = useState<PageState>("voting")
  const [votedValue, setVotedValue] = useState<number | undefined>()
  const [voteAction, setVoteAction] = useState<VoteAction | undefined>()
  const [commentSubmitting, setCommentSubmitting] = useState(false)

  const { isFetching: isCommentsFetching, data: comments } = useGetProposalComments(storeId, proposal?.id)

  const NestedContent = proposal?.operation ? renderByOperationType[proposal.operation] : undefined
  const isPublicationProposal = !!NestedContent

  const productId = useMemo(
    () => (isPublicationProposal ? getProductId(proposal) : undefined),
    [isPublicationProposal, proposal],
  )
  const publicationId = useMemo(
    () => (isPublicationProposal ? getPublicationId(proposal) : undefined),
    [isPublicationProposal, proposal],
  )

  const headerBreadcrumbs = useMemo(
    () =>
      parentBreadcrumbs
        ? Array.isArray(parentBreadcrumbs)
          ? [...parentBreadcrumbs]
          : [parentBreadcrumbs]
        : { title: t("common:proposals"), path: routes.moderation.proposals(storeId!) },
    [parentBreadcrumbs, storeId, t],
  )

  const togglePageState = useCallback(() => setPageState(prev => (prev === "voting" ? "results" : "voting")), [])

  const handleVoteClick = useCallback(
    (value: number) => {
      if (isReferendum && !approval) {
        startSignIn("author")
        return
      }

      setVotedValue(value)
      setVoteStatus("voting")
      const operation = new ProposalVoting(proposal!.id, approval!.id, value)
      mutate(operation, {
        onSuccess: () => {
          showToast(t("toast:voted"), "success")
          setVoteStatus("voted")

          invalidateOperation(proposal!.operation, { storeId: storeId! })
          invalidateProposal({ storeId: storeId!, proposalId: proposal!.id })

          navigate(previousPath ?? routes.moderation.proposals(storeId!))
        },
        onError: err => {
          showToast(err.toString(), "error")
          setVoteStatus("idle")
        },
        onSettled: () => setVotedValue(undefined),
      })
    },
    [
      isReferendum,
      approval,
      proposal,
      mutate,
      startSignIn,
      t,
      invalidateOperation,
      invalidateProposal,
      navigate,
      previousPath,
      storeId,
    ],
  )

  const vote = useCallback(
    (action: VoteAction) => {
      setVoteStatus("voting")
      setVoteAction(action)
      const operation = new ProposalVoting(proposal!.id, approval!.id, action === "approve" ? 0 : SpecialChoice.Neither)
      mutate(operation, {
        onSuccess: () => {
          showToast(t("toast:publicationVoted"), "success")
          invalidateProposal({ storeId: storeId!, proposalId: proposal!.id })
          navigate(routes.moderation.publications(storeId!, "proposals"))
        },
        onError: err => {
          showToast(err.toString(), "error")
          setVoteStatus("idle")
          setVoteAction(undefined)
        },
      })
    },
    [mutate, navigate, proposal, storeId, t, approval, invalidateProposal],
  )

  const handleApprove = useCallback(() => vote("approve"), [vote])
  const handleReject = useCallback(() => vote("reject"), [vote])

  const handleCommentSubmit = useCallback(
    (comment: string) => {
      setCommentSubmitting(true)
      if (!isPublicationProposal) setVotedValue(ALREADY_VOTED)
      const operation = new ProposalCommentCreation(proposal!.id, approvalId!, comment)
      mutate(operation, {
        onSuccess: () => {
          invalidateProposal({ storeId: storeId!, proposalId: proposal!.id })
          showToast(t("toast:commentAdded"), "success")
        },
        onError: err => showToast(err.toString(), "error"),
        onSettled: () => {
          setCommentSubmitting(false)
          if (!isPublicationProposal) setVotedValue(undefined)
        },
      })
    },
    [isPublicationProposal, proposal, approvalId, mutate, invalidateProposal, storeId, t],
  )

  useEffect(() => {
    if (!approval) return

    if (isPublicationProposal) {
      if (isVoted(approval.id, proposal)) {
        setVoteStatus("voted")
      }
    } else {
      const votedIndex = getVotedIndex(approval.id, proposal)
      if (votedIndex !== undefined) {
        setVoteStatus("voted")
        setVotedValue(votedIndex)
      }
    }
  }, [isPublicationProposal, proposal, approval, productId])

  if (!proposal || !comments) {
    return <div>Loading{import.meta.env.DEV ? " (ProposalView)" : ""}</div>
  }

  return (
    <div className="flex flex-col gap-6">
      <ModerationHeader
        title={proposal.title ?? proposal.id}
        subtitle={t(`policySurveysPage:${proposal?.operation}`)}
        parentBreadcrumbs={headerBreadcrumbs}
        components={
          isPublicationProposal && !!approval && voteStatus !== "voted" ? (
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
                to={routes.moderation.preview(storeId!)}
                state={{
                  productId,
                  publicationId,
                  proposalId: proposal.id,
                  previousPath: routes.moderation.moderatorPublication(storeId!, proposal.id),
                  parentBreadcrumbs,
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
        {...headerProps}
      />
      {NestedContent && <NestedContent t={t} storeId={storeId!} proposal={proposal} voteStatus={voteStatus} />}
      <div className="flex gap-8">
        <div className="flex w-full flex-col gap-8">
          {!isPublicationProposal && (
            <DefaultContent
              t={t}
              storeId={storeId!}
              proposal={proposal}
              showVoteButton={!!approval && voteStatus !== "voted"}
              pageState={pageState}
              voteStatus={voteStatus}
              votedValue={votedValue}
              onVoteClick={handleVoteClick}
            />
          )}
          {!isPublicationProposal && !!approval && !isReferendum && (
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
            showCommentInput={canCreateComment}
            isFetching={isCommentsFetching}
            comments={comments}
            onCommentSubmit={handleCommentSubmit}
          />
        </div>
        <div className="flex flex-col gap-6">
          <ProposalInfo
            className="w-87.5"
            store={store!}
            proposal={proposal}
            policies={policies!}
            voterIds={voterIds}
          />
          {!isPublicationProposal &&
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
