import { ReactNode } from "react"
import { TFunction } from "i18next"

import { AuthorBaseAvatar, OperationType, Proposal, PublicationImageBase, PublicationProposal } from "types"
import { ButtonOutline, ButtonPrimary, MemberInfo, PublicationInfo, TableColumn } from "ui/components"
import {
  formatNabb,
  formatNabbShort,
  formatDuration,
  getHoursPassedFromStart,
  formatArShort,
  formatAr,
  buildFileUrl,
} from "utils"
import { renderVotes } from "ui/renderers2"

const FONT_SM_CLASSNAME = "text-sm leading-4.25"

export const renderAuthor2 = (authorTitle: string, authorLogoId?: string) => (
  <MemberInfo title={authorTitle} avatarSrc={buildFileUrl(authorLogoId)} />
)

export const renderAuthor = (author: AuthorBaseAvatar) => (
  <MemberInfo
    title={
      author.title && author.name ? `${author.title} (${author.name})` : (author.title ?? author.name ?? author.id)
    }
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
  const hoursPassed = getHoursPassedFromStart()
  const hoursDuration = hoursPassed - creationTime
  const formatted = formatDuration(t, hoursDuration)
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
