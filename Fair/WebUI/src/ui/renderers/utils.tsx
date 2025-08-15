import { TFunction } from "i18next"
import { ReactNode } from "react"

import { AccountBase, BaseProposal, OperationType, PublicationImageBase } from "types"
import { AccountInfo, PublicationInfo, TableColumn } from "ui/components"
import { formatDate, formatDuration, formatVotes, getDaysPassedFromStart, shortenAddress } from "utils"

const FONT_SM_CLASSNAME = "text-sm leading-4.25"

export const renderAccount = (account: AccountBase) => (
  <AccountInfo
    title={account.nickname || shortenAddress(account.address)}
    fullTitle={account.nickname || account.address}
    avatar={account.avatar}
  />
)

export const renderAction = (t: TFunction, operationType: OperationType) => t(`operations:${operationType}`)

export const renderActionShort = (t: TFunction, operationType: OperationType) => (
  <span title={t(`operations:${operationType}`)}>{t(`operationsShort:${operationType}`)}</span>
)

export const renderAnbb = (t: TFunction, proposal: BaseProposal) => (
  <div className="flex items-center gap-1">
    <span title={t("common:abstained")}>{proposal.abstainedCount}</span>
    {" / "}
    <span title={t("common:neither")}>{proposal.neitherCount}</span>
    {" / "}
    <span title={t("common:ban")}>{proposal.banCount}</span>
    {" / "}
    <span title={t("common:banish")}>{proposal.banishCount}</span>
  </div>
)

export const renderDate = (date: number) => <span className={FONT_SM_CLASSNAME}>{formatDate(date)}</span>

export const renderDaysLeft = (creationTime: number, expirationTime: number) => (
  <span className={FONT_SM_CLASSNAME}>{expirationTime - creationTime}</span>
)

export const renderLastsFor = (t: TFunction, creationTime: number) => {
  const passed = getDaysPassedFromStart()
  const duration = passed - creationTime + 1
  const formatted = formatDuration(t, duration)
  return <span className={FONT_SM_CLASSNAME}>{formatted}</span>
}

export const renderPublication = (publication: PublicationImageBase) => (
  <PublicationInfo avatar={publication.image} categoryTitle={publication.categoryTitle} title={publication.title} />
)

export const renderText = (text: string) => (
  <span className="line-clamp-2 break-words text-sm leading-4.75" title={text}>
    {text}
  </span>
)

export const renderTitle = (title: string, text: string) => (
  <div className="flex flex-col gap-1" title={title}>
    <span className="overflow-hidden text-ellipsis whitespace-nowrap text-2sm leading-5">{title}</span>
    <span className="overflow-hidden text-ellipsis whitespace-nowrap text-2xs leading-4 text-gray-500">{text}</span>
  </div>
)

export const renderVotes = (votes: number[]) => {
  const formatted = formatVotes(votes)
  return (
    <div className="overflow-hidden text-ellipsis whitespace-nowrap" title={formatted}>
      {formatted}
    </div>
  )
}

export const renderCommon = (t: TFunction, column: TableColumn, proposal: BaseProposal): ReactNode => {
  switch (column.type) {
    case "anbb":
      return renderAnbb(t, proposal)
    case "lasts-for":
      return renderLastsFor(t, proposal.creationTime)
    case "votes":
      return renderVotes(proposal.optionVotesCount)
  }

  return undefined
}
