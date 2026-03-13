import { ReactNode, MouseEvent } from "react"
import { TFunction } from "i18next"

import { AccountBaseAvatar, BaseProposal, OperationType, PublicationImageBase } from "types"
import { AccountInfo, ButtonOutline, ButtonPrimary, PublicationInfo, TableColumn } from "ui/components"
import {
  formatNabb,
  formatNabbShort,
  formatDate,
  formatDuration,
  formatVotes,
  getDaysPassedFromStart,
  shortenAddress,
} from "utils"

const FONT_SM_CLASSNAME = "text-sm leading-4.25"

export const renderAccount = (account: AccountBaseAvatar) => (
  <AccountInfo
    title={account.nickname || shortenAddress(account.address)}
    fullTitle={account.nickname || account.address}
    avatar={account.avatar}
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

export const renderNabb = (t: TFunction, proposal: BaseProposal) => {
  const title = formatNabb(t, proposal.neitherCount, proposal.anyCount, proposal.banCount, proposal.banishCount)
  const value = formatNabbShort(proposal.neitherCount, proposal.anyCount, proposal.banCount, proposal.banishCount)
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

export const renderDate = (date: number) => <span className={FONT_SM_CLASSNAME}>{formatDate(date)}</span>

export const renderLastsFor = (t: TFunction, creationTime: number) => {
  const passed = getDaysPassedFromStart()
  const duration = passed - creationTime + 1
  const formatted = formatDuration(t, duration)
  return (
    <span className={FONT_SM_CLASSNAME} title={formatted}>
      {formatted}
    </span>
  )
}

export const renderPublication = (publication: PublicationImageBase) => (
  <PublicationInfo avatarId={publication.imageId} categoryTitle={publication.categoryTitle} title={publication.title} />
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

export const renderVotes = (votes: number[]) => {
  const formatted = formatVotes(votes)
  return (
    <div className="truncate" title={formatted}>
      {formatted}
    </div>
  )
}

export const renderCommon = (t: TFunction, column: TableColumn, proposal: BaseProposal): ReactNode => {
  switch (column.type) {
    case "nabb":
      return renderNabb(t, proposal)
    case "lasts-for":
      return renderLastsFor(t, proposal.creationTime)
    case "votes":
      return renderVotes(proposal.optionsVotesCount)
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
  <div className="flex gap-5">
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
