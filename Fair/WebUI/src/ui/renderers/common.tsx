import { SvgStarXxs } from "assets"
import { AccountBase, PublicationImageBase } from "types"
import { AccountInfo, ButtonOutline, ButtonPrimary, PublicationInfo } from "ui/components"
import { formatDate, shortenAddress } from "utils"

export const renderAccount = (account: AccountBase) => (
  <AccountInfo
    title={account.nickname || shortenAddress(account.address)}
    fullTitle={account.nickname || account.address}
    avatar={account.avatar}
  />
)

export const renderApproveRejectAction = () => (
  <div className="flex gap-4">
    <ButtonPrimary className="h-9 w-20" label="Approve" />
    <ButtonOutline className="h-9 w-20" label="Reject" />
  </div>
)

export const renderDate = (days: number) => <span className="text-sm leading-4.25">{formatDate(days)}</span>

export const renderPublication = (publication: PublicationImageBase) => (
  <PublicationInfo avatar={publication.image} categoryTitle={publication.categoryTitle} title={publication.title} />
)

export const renderText = (text: string) => (
  <span className="leading-4.75 line-clamp-2 break-words text-sm">{text}</span>
)

export const renderRating = (rating: number) => (
  <div className="flex items-center gap-1">
    <span className="text-sm font-medium leading-4.25">{rating / 10}</span>
    <SvgStarXxs className="stroke-gray-800" />
  </div>
)
