import { memo } from "react"
import { useTranslation } from "react-i18next"
import { twMerge } from "tailwind-merge"

import { Creator, PropsWithClassName } from "types"
import { LinkFullscreen } from "ui/components"
import { renderCreator } from "ui/renderers2"
import { buildCreatorLink, getLastsFor, formatDate } from "utils"

type ProposalInfoBaseProps = {
  storeId: string
  createdBy: Creator
  createdAt: number
}

export type ProposalInfoProps = ProposalInfoBaseProps & PropsWithClassName

export const ProposalInfo = memo(({ className, storeId, createdBy, createdAt }: ProposalInfoProps) => {
  const { t } = useTranslation()

  return (
    <div
      className={twMerge(
        "flex max-h-fit flex-col gap-4 rounded-lg border border-gray-300 bg-gray-100 p-6 text-2sm leading-5 text-gray-800",
        className,
      )}
    >
      <div className="flex flex-col gap-2">
        <span className="text-gray-500">{t("common:labels.createdBy")}</span>
        <LinkFullscreen to={buildCreatorLink(storeId, createdBy)}>{renderCreator(createdBy)}</LinkFullscreen>
      </div>
      <div className="flex flex-col gap-2">
        <span className="text-gray-500">{t("common:labels.createdAt")}</span>
        <span>{formatDate(createdAt)}</span>
      </div>
      <div className="flex flex-col gap-2">
        <span className="text-gray-500">{t("common:labels.lastsFor")}</span>
        <span>{getLastsFor(t, createdAt)}</span>
      </div>
    </div>
  )
})
