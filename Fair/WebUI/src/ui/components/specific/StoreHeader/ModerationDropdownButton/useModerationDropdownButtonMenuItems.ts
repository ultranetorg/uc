import { useMemo } from "react"
import { useTranslation } from "react-i18next"

import { useStoreChangeMenuItems } from "hooks"
import { routes } from "utils"
import { SimpleMenuItem } from "ui/components"

export const useModerationDropdownButtonMenuItems = (storeId: string): SimpleMenuItem[] => {
  const { t } = useTranslation("storeDropdownMenu")

  const storeItems = useStoreChangeMenuItems(storeId, { publishers: false })

  const menuItems = useMemo(
    () => [
      { label: t("common:proposals"), to: routes.moderation.proposals(storeId) },
      { label: t("common:moderators"), to: routes.moderation.moderators(storeId) },
      { label: t("common:publications"), to: routes.moderation.publications(storeId) },
      { label: t("common:publishers"), to: routes.moderation.publishers(storeId) },
      { label: t("common:reviews"), to: routes.moderation.reviews(storeId) },
      { label: t("common:users"), to: routes.moderation.users(storeId) },
      ...(storeItems.length > 0 ? [{ separator: true }, ...storeItems] : []),
    ],
    [storeId, storeItems, t],
  )

  return menuItems
}
