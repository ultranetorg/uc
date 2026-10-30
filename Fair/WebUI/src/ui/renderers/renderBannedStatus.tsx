import { TFunction } from "i18next"

import { Badge } from "ui/components"
import { formatDate } from "utils"

export const renderBannedStatus = (t: TFunction, bannedTill: number) =>
  bannedTill !== 0 ? (
    <Badge variant="warning">{t("common:bannedUntil", { date: formatDate(bannedTill) })}</Badge>
  ) : (
    <span className="text-gray-500">{t("common:active")}</span>
  )
