import { useTranslation } from "react-i18next"
import { Link } from "react-router-dom"
import { twMerge } from "tailwind-merge"

import { useStoreRolesContext } from "app"
import { useResolveStoreId } from "hooks"
import { PropsWithClassName } from "types"
import { routes } from "utils"

import { HeaderDropdownButton } from "../HeaderDropdownButton"
import { MENU_ITEM_STYLE } from "../styles"

import { useModerationDropdownButtonMenuItems } from "./useModerationDropdownButtonMenuItems"

export const ModerationDropdownButton = ({ className }: PropsWithClassName) => {
  const storeId = useResolveStoreId()
  const { isPublisher, isModerator } = useStoreRolesContext()
  const { t } = useTranslation()

  const menuItems = useModerationDropdownButtonMenuItems(storeId!)

  if (!isPublisher && !isModerator) return null

  return isPublisher && !isModerator ? (
    <Link to={routes.moderation.moderators(storeId!)} className={twMerge(MENU_ITEM_STYLE, "w-22")}>
      {t("common:moderators")}
    </Link>
  ) : (
    <HeaderDropdownButton
      className={twMerge(className, "first-letter:uppercase")}
      label={t("common:moderation")}
      menuItems={menuItems}
    />
  )
}
