import { ComponentType, memo, useCallback, useEffect, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"
import { Link, useNavigate } from "react-router-dom"
import { useQueryClient } from "@tanstack/react-query"

import { useOperationPolicy, useSignInContext } from "app"
import { SvgArrowLeft, SvgEyeSm } from "assets"
import {
  categoriesKeys,
  proposalsKeys,
  publicationsKeys,
  storesKeys,
  useGetModeratorDiscussionComments,
} from "entities"
import { useTransactMutationWithStatus } from "entities/iccpNode"
import { useResolveStoreId } from "hooks"
import { OperationType, ProposalCommentCreation, ProposalDetails, ProposalVoting, SpecialChoice } from "types"
import { BreadcrumbsItemProps, ButtonBar, ButtonOutline, ButtonPrimary, Separator } from "ui/components"
import { AlternativeOptions, CommentsSection, ProposalInfo } from "ui/components/proposal"
import { ModerationHeader } from "ui/components/specific"
import { getVotedIndex, isVoted, routes, showToast } from "utils"

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

const renderByOperationType: Partial<Record<OperationType, ComponentType<ProposalViewContentProps>>> = {
  "publication-creation": PublicationCreationContent,
  "publication-updation": PublicationUpdationContent,
  "publication-deletion": PublicationDeletionContent,
}

// Set voted value in order to disable all buttons inside OptionsCollapsesList.
const ALREADY_VOTED = 100

export type ProposalViewProps = {
  parentBreadcrumbs?: BreadcrumbsItemProps | BreadcrumbsItemProps[]
  isFetching: boolean
  proposal?: ProposalDetails
  previousPath?: string
}

export const ProposalView = memo(({ parentBreadcrumbs, proposal, previousPath }: ProposalViewProps) => {
  const storeId = useResolveStoreId()
  const { voter: approval, policy } = useOperationPolicy(proposal?.operation)
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const { mutate } = useTransactMutationWithStatus()
  const { t } = useTranslation("proposalView")

  const { startSignIn } = useSignInContext()

  const isReferendum = policy?.approval === "publishers-majority"

  const [voteStatus, setVoteStatus] = useState<VoteStatus>("idle")
  const [pageState, setPageState] = useState<PageState>("voting")
  const [votedValue, setVotedValue] = useState<number | undefined>()
  const [voteAction, setVoteAction] = useState<VoteAction | undefined>()
  const [commentSubmitting, setCommentSubmitting] = useState(false)

  const invalidateQueryKeysByOperationType: Partial<Record<OperationType, readonly (readonly string[])[]>> = useMemo(
    () => ({
      "site-moderator-removal": [storesKeys.moderators(storeId!), proposalsKeys.moderators(storeId!)],
      "site-authors-removal": [storesKeys.publishers(storeId!), proposalsKeys.publishers(storeId!)],

      "site-avatar-change": [storesKeys.detail(storeId!)],
      "site-name-change": [storesKeys.detail(storeId!)],
      "site-text-change": [storesKeys.detail(storeId!)],

      "category-creation": [categoriesKeys.all(storeId!)],
      "publication-deletion": [publicationsKeys.categoriesPublications(storeId!)],
      "publication-unpublish": [publicationsKeys.categoriesPublications(storeId!)],
    }),
    [storeId],
  )

  const {
    isFetching: isCommentsFetching,
    data: comments,
    refetch: refetchComments,
  } = useGetModeratorDiscussionComments(storeId, proposal?.id)

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

          const invalidateKeys = invalidateQueryKeysByOperationType[proposal!.operation]
          if (invalidateKeys) {
            invalidateKeys.forEach(x => queryClient.invalidateQueries({ queryKey: x }))
          }

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
      invalidateQueryKeysByOperationType,
      navigate,
      previousPath,
      storeId,
      queryClient,
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
          navigate(routes.moderation.publications(storeId!, "proposals"))
        },
        onError: err => {
          showToast(err.toString(), "error")
          setVoteStatus("idle")
          setVoteAction(undefined)
        },
      })
    },
    [mutate, navigate, proposal, storeId, t, approval],
  )

  const handleApprove = useCallback(() => vote("approve"), [vote])
  const handleReject = useCallback(() => vote("reject"), [vote])

  const handleCommentSubmit = useCallback(
    (comment: string) => {
      setCommentSubmitting(true)
      if (!isPublicationMode) setVotedValue(ALREADY_VOTED)
      const operation = new ProposalCommentCreation(proposal!.id, approval!.id, comment)
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
    [isPublicationMode, mutate, proposal, refetchComments, t, approval],
  )

  useEffect(() => {
    if (!approval) return

    if (isPublicationMode) {
      if (isVoted(approval!.id, proposal)) {
        setVoteStatus("voted")
      }
    } else {
      const votedIndex = getVotedIndex(approval!.id, proposal)
      if (votedIndex !== undefined) {
        setVoteStatus("voted")
        setVotedValue(votedIndex)
      }
    }
  }, [isPublicationMode, proposal, approval])

  if (!proposal || !comments) {
    return <div>Loading</div>
  }

  return (
    <div className="flex flex-col gap-6">
      <ModerationHeader
        title={proposal.title ?? proposal.id}
        parentBreadcrumbs={headerBreadcrumbs}
        components={
          isPublicationMode && !!approval && voteStatus !== "voted" ? (
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
      />
      {NestedContent && <NestedContent t={t} storeId={storeId!} proposal={proposal} voteStatus={voteStatus} />}
      <div className="flex gap-8">
        <div className="flex w-full flex-col gap-8">
          {!isPublicationMode && (
            <DefaultContent
              t={t}
              storeId={storeId!}
              proposal={proposal}
              isReferendum={isReferendum}
              pageState={pageState}
              voteStatus={voteStatus}
              votedValue={votedValue}
              onVoteClick={handleVoteClick}
            />
          )}
          {!isPublicationMode && !!approval && (
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
            showCommentInput={!!approval}
            isFetching={isCommentsFetching}
            comments={comments}
            onCommentSubmit={handleCommentSubmit}
          />
        </div>
        <div className="flex flex-col gap-6">
          <ProposalInfo
            className="w-87.5"
            storeId={storeId!}
            createdBy={proposal.by}
            createdAt={proposal.creationTime}
          />
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
