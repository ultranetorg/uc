import { memo } from "react"
import { useTranslation } from "react-i18next"
import { Link } from "react-router-dom"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName, User } from "types"
import { renderUser } from "ui/renderers2"
import { formatDate, formatLastsFor, routes } from "utils"

type ProposalInfoBaseProps = {
  createdBy: User
  createdAt: number
}

export type ProposalInfoProps = ProposalInfoBaseProps & PropsWithClassName

export const ProposalInfo = memo(({ className, createdBy, createdAt }: ProposalInfoProps) => {
  const { t } = useTranslation()

  return (
    <div
      className={twMerge(
        "flex max-h-fit flex-col gap-4 rounded-lg border border-gray-300 bg-gray-100 p-6 text-2sm leading-5 text-gray-800",
        className,
      )}
    >
      <div className="flex flex-col gap-2">
        <span className="text-gray-500">Created By:</span>
        <Link to={routes.publisher(createdBy.id)}>{renderUser(createdBy)}</Link>
      </div>
      <div className="flex flex-col gap-2">
        <span className="text-gray-500">Created At:</span>
        <span>{formatDate(createdAt)}</span>
      </div>
      <div className="flex flex-col gap-2">
        <span className="text-gray-500">Lasts for:</span>
        <span>{formatLastsFor(t, createdAt)}</span>
      </div>
    </div>
  )
})
