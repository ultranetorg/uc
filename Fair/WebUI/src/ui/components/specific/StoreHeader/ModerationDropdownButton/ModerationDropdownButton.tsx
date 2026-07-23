import { useTranslation } from "react-i18next"
import { twMerge } from "tailwind-merge"

import { useResolveStoreId } from "hooks"
import { PropsWithClassName } from "types"

import { HeaderDropdownButton } from "../HeaderDropdownButton"

import { useModerationDropdownButtonMenuItems } from "./useModerationDropdownButtonMenuItems"

export const ModerationDropdownButton = ({ className }: PropsWithClassName) => {
  const storeId = useResolveStoreId()
  const { t } = useTranslation()

  const menuItems = useModerationDropdownButtonMenuItems(storeId!)

  return (
    <HeaderDropdownButton
      className={twMerge(className, "first-letter:uppercase")}
      label={t("common:moderation")}
      menuItems={menuItems}
    />
  )
}
