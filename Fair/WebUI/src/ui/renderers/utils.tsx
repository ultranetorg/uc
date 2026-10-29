import { ReactNode } from "react"
import { TFunction } from "i18next"

import avatarFallback from "assets/fallback/author-16.png"
import {
  AuthorBaseAvatar,
  OperationType,
  Proposal,
  PublicationImageBase,
  PublicationProposal,
  SpecialChoice,
} from "types"
import {
  ButtonOutline,
  ButtonPrimary,
  MemberInfo,
  NotVotedBadge,
  PublicationInfo,
  TableColumn,
  VotesProgress,
} from "ui/components"
import { renderVotes } from "ui/renderers2"
import {
  formatNabb,
  formatNabbShort,
  formatArShort,
  formatAr,
  buildFileUrl,
  getLastsFor,
  getLeadingOptionIndexes,
  getLeadingVotes,
  getVotedChoice,
} from "utils"

const FONT_SM_CLASSNAME = "text-sm leading-4.25"

export const renderAuthor2 = (authorTitle: string, authorLogoId?: string) => (
  <MemberInfo title={authorTitle} fallbackSrc={avatarFallback} avatarSrc={buildFileUrl(authorLogoId)} />
)

export const renderAuthor = (author: AuthorBaseAvatar) => (
  <MemberInfo
    title={
      author.title && author.name ? `${author.title} (${author.name})` : (author.title ?? author.name ?? author.id)
    }
    fallbackSrc={avatarFallback}
    avatarSrc={buildFileUrl(author.avatarId)}
  />
)

export const renderAction = (t: TFunction, operationType: OperationType) => {
  const value = t(`operations:${operationType}`)
  return (
    <div title={value} className="truncate">
      {value}
    </div>
  )
}

export const renderActionShort = (t: TFunction, operationType: OperationType) => (
  <span title={t(`operations:${operationType}`)}>{t(`operationsShort:${operationType}`)}</span>
)

export const renderAr = (t: TFunction, proposal: PublicationProposal) => {
  const title = formatAr(t, proposal.yes[0].length, proposal.neither.length)
  const value = formatArShort(proposal.yes[0].length, proposal.neither.length)
  return (
    <div className="truncate" title={title}>
      {value}
    </div>
  )
}

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

export const renderCategory = (title: string) => (
  <div className="truncate" title={title}>
    {title}
  </div>
)

export const renderLastsFor = (t: TFunction, creationTime: number) => {
  const formatted = getLastsFor(t, creationTime)
  return (
    <span className={FONT_SM_CLASSNAME} title={formatted}>
      {formatted}
    </span>
  )
}

export const renderPublication = (publication: PublicationImageBase) => (
  <PublicationInfo avatarId={publication.imageId} categoryTitle={publication.categoryTitle} title={publication.title} />
)

export const renderPublication2 = (title?: string, logoId?: string) => (
  <PublicationInfo avatarId={logoId} title={title} />
)

export const renderText = (text: string) => (
  <span className="line-clamp-2 break-words text-sm leading-4.75" title={text}>
    {text}
  </span>
)

export const renderTitle = (title: string, text: string) => (
  <div className="flex flex-col gap-1" title={title}>
    <span className="truncate text-2sm leading-5">{title}</span>
    <span className="truncate text-2xs leading-4 text-gray-500">{text}</span>
  </div>
)

export const renderCommon = (
  t: TFunction,
  column: TableColumn,
  proposal: Proposal,
  votesRequired?: number,
): ReactNode => {
  switch (column.type) {
    case "nabb":
      return renderNabb(t, proposal)
    case "lasts-for":
      return renderLastsFor(t, proposal.creationTime)
    case "votes":
      return renderVotes(
        proposal.yes.map(x => x.length),
        votesRequired,
      )
  }

  return undefined
}

export const renderActions = (
  t: TFunction,
  onApprove: () => void,
  onReject: () => void,
  loadingAction?: "approve" | "reject",
  locked?: boolean,
) => (
  <div className="flex justify-end gap-5">
    <ButtonPrimary
      className="h-9 w-20 capitalize"
      label={t("common:approve")}
      loading={loadingAction === "approve"}
      disabled={loadingAction === "reject" || locked}
      onClick={e => {
        onApprove()
        e.stopPropagation()
        e.preventDefault()
      }}
    />
    <ButtonOutline
      className="h-9 w-20 capitalize"
      label={t("common:reject")}
      loading={loadingAction === "reject"}
      disabled={loadingAction === "approve" || locked}
      onClick={e => {
        onReject()
        e.stopPropagation()
        e.preventDefault()
      }}
    />
  </div>
)

const SPECIAL_CHOICE_KEYS: Record<number, string> = {
  [SpecialChoice.Neither]: "common:neither",
  [SpecialChoice.Any]: "common:any",
  [SpecialChoice.Ban]: "common:ban",
  [SpecialChoice.Banish]: "common:banish",
}

const getOptionTitle = (t: TFunction, proposal: Proposal, index: number): string =>
  proposal.yes.length === 1
    ? t("common:yes")
    : proposal.optionTitles?.[index] || t("common:optionNumber", { number: index + 1 })

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

export const renderProposalProgress = (t: TFunction, proposal: Proposal, votesRequired: number) => {
  const leadingVotes = getLeadingVotes(proposal)
  const votes = leadingVotes + proposal.any.length

  return votesRequired > 0 ? (
    <VotesProgress
      votes={votes}
      votesRequired={votesRequired}
      title={`${leadingVotes} + ${t("common:any")}: ${proposal.any.length}`}
    />
  ) : (
    <span>{votes}</span>
  )
}

export const renderYourVote = (t: TFunction, proposal: Proposal, voterIds: string[]) => {
  if (voterIds.length === 0) return "-"

  const choice = getVotedChoice(voterIds, proposal)
  if (choice === undefined) {
    return <NotVotedBadge />
  }

  const text = choice >= 0 ? getOptionTitle(t, proposal, choice) : t(SPECIAL_CHOICE_KEYS[choice])
  return (
    <div className="truncate" title={text}>
      {text}
    </div>
  )
}
