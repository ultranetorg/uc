import { Link, useParams } from "react-router-dom"
import { twMerge } from "tailwind-merge"

import { AccountBase, PropsWithClassName } from "types"
import { AccountInfo } from "ui/components"
import { formatDate, shortenAddress } from "utils"

type ProposalInfoBaseProps = {
  createdBy: AccountBase
  createdAt: number
  daysLeft: number
}

export type ProposalInfoProps = ProposalInfoBaseProps & PropsWithClassName

export const ProposalInfo = ({ className, createdBy, createdAt, daysLeft }: ProposalInfoProps) => {
  const { siteId } = useParams()

  return (
    <div
      className={twMerge(
        "flex max-h-fit flex-col gap-4 rounded-lg border border-gray-300 bg-gray-100 p-6 text-2sm leading-5 text-gray-800",
        className,
      )}
    >
      <div className="flex flex-col gap-2">
        <span className="text-gray-500">Created By:</span>
        <Link to={`/${siteId}/a/${createdBy.id}`}>
          <AccountInfo
            title={createdBy.nickname || shortenAddress(createdBy.address)}
            fullTitle={createdBy.nickname || createdBy.address}
            avatar={createdBy.avatar}
          />
        </Link>
      </div>
      <div className="flex flex-col gap-2">
        <span className="text-gray-500">Created At:</span>
        <span>{formatDate(createdAt)}</span>
      </div>
      <div className="flex flex-col gap-2">
        <span className="text-gray-500">Days Left:</span>
        <span>{daysLeft}</span>
      </div>
    </div>
  )
}
