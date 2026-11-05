import { TFunction } from "i18next"

import { Badge } from "ui/components"
import { formatRole } from "utils"

// Бейдж «Not a Member» показывается только рядом с ролью User: паблишеры и модераторы всегда участники стора.
export const renderRole = (t: TFunction, isPublisher: boolean, isModerator: boolean, isJoined?: boolean) => {
  const isUserRole = !isPublisher && !isModerator

  return (
    <div className="flex items-center gap-2">
      <span className="capitalize">{formatRole(t, isPublisher, isModerator)}</span>
      {isUserRole && isJoined === false && <Badge variant="warning">{t("userDetailsView:notMember")}</Badge>}
    </div>
  )
}
