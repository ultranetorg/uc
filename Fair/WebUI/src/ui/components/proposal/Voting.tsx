import { TFunction } from "i18next"

import { AccountInfo, ButtonPrimary, LinkFullscreen } from "ui/components"

export type VotingProps = {
  t: TFunction
  siteId: string
  daysLeft: number
  createdAt: number
  createdById: string
  createdByTitle: string
  createdByAvatar?: string
}

export const Voting = ({
  t,
  siteId,
  daysLeft,
  createdAt,
  createdById,
  createdByTitle,
  createdByAvatar,
}: VotingProps) => (
  <div className="flex flex-col gap-8 rounded-lg border border-gray-300 bg-gray-100 p-6">
    <div className="flex flex-col gap-4 text-2sm leading-5">
      <div className="flex flex-col gap-2">
        <span className="text-gray-500">{t("daysLeft")}</span>
        <span>{daysLeft}</span>
      </div>
      <div className="flex flex-col gap-2">
        <span className="text-gray-500">{t("createdAt")}</span>
        <span>{createdAt}</span>
      </div>
      <div className="flex flex-col gap-2">
        <span className="text-gray-500">{t("createdBy")}</span>
        <LinkFullscreen className="w-fit" to={`/${siteId}/a/${createdById}`}>
          <AccountInfo
            title={createdByTitle}
            fullTitle={createdByTitle}
            avatar={createdByAvatar}
            titleClassName="hover:font-semibold"
          />
        </LinkFullscreen>
      </div>
    </div>
    <div className="flex flex-col gap-4">
      <span className="font-medium">Vote</span>
      <ButtonPrimary label={t("common:yes")} />
      <ButtonPrimary label={t("common:no")} />
      <ButtonPrimary label={t("common:abs")} />
    </div>
  </div>
)
