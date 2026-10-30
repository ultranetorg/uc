import { TFunction } from "i18next"

import { Badge } from "ui/components"
import { renderUser } from "ui/renderers2"

export const renderModerator = (t: TFunction, id: string, name: string, currentUserId?: string) => (
  <div className="flex items-center gap-2">
    {renderUser(id, name)}
    {id === currentUserId && <Badge>{t("common:you")}</Badge>}
  </div>
)
