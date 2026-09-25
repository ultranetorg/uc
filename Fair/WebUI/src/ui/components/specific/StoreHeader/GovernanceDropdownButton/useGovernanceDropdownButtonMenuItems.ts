import { useMemo } from "react"
import { useTranslation } from "react-i18next"

import { useStoreChangeMenuItems } from "hooks"
import { routes } from "utils"
import { SimpleMenuItem } from "ui/components/SimpleMenu"

export const useGovernanceDropdownButtonMenuItems = (storeId: string): SimpleMenuItem[] => {
  const { t } = useTranslation("storeDropdownMenu")

  const storeItems = useStoreChangeMenuItems(storeId, { moderators: false })

  const menuItems = useMemo(
    () => [
      { label: t("common:surveys"), to: routes.governance.surveys(storeId) },
      { label: t("common:referendums"), to: routes.governance.referendums(storeId) },
      ...(storeItems.length > 0 ? [{ separator: true }, ...storeItems] : []),
    ],
    [storeId, storeItems, t],
  )

  return menuItems
}
