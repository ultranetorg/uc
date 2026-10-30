import { ReactNode } from "react"
import { TFunction } from "i18next"

import { Badge } from "ui/components"

export const renderWithYouBadge = (t: TFunction, content: ReactNode, isYou: boolean) =>
  isYou ? (
    <div className="flex items-center gap-2">
      {content}
      <Badge>{t("common:you")}</Badge>
    </div>
  ) : (
    content
  )
