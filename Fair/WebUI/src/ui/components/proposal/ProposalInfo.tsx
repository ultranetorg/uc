import { memo, PropsWithChildren } from "react"
import { useTranslation } from "react-i18next"
import { twMerge } from "tailwind-merge"

import { Creator, PropsWithClassName } from "types"
import { LinkFullscreen } from "ui/components"
import { renderCreator } from "ui/renderers2"
import { buildCreatorLink, getLastsFor, formatDate } from "utils"

type InfoRowBaseProps = {
  label: string
}

type InfoRowProps = PropsWithChildren & InfoRowBaseProps

const InfoRow = ({ label, children }: InfoRowProps) => (
  <div className="flex flex-col gap-2">
    <span className="text-gray-500">{label}</span>
    {children}
  </div>
)

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
      <InfoRow label={t("labels:createdBy")}>
        <LinkFullscreen to={buildCreatorLink(storeId, createdBy)}>{renderCreator(createdBy)}</LinkFullscreen>
      </InfoRow>
      <InfoRow label={t("labels:createdAt")}>
        <span>{formatDate(createdAt)}</span>
      </InfoRow>
      <InfoRow label={t("labels:lastsFor")}>
        <span>{getLastsFor(t, createdAt)}</span>
      </InfoRow>
    </div>
  )
})
