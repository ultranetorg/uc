import { useTranslation } from "react-i18next"
import { twMerge } from "tailwind-merge"

import { useResolveStoreId } from "hooks"
import { PropsWithClassName } from "types"

import { HeaderDropdownButton } from "../HeaderDropdownButton"

import { useStoreDropdownButtonMenuItems } from "./useStoreDropdownButtonMenuItems"

export const StoreDropdownButton = ({ className }: PropsWithClassName) => {
  const storeId = useResolveStoreId()
  const { t } = useTranslation()

  const menuItems = useStoreDropdownButtonMenuItems(storeId!)

  return (
    <HeaderDropdownButton
      className={twMerge(className, "first-letter:uppercase")}
      label={t("common:store")}
      menuItems={menuItems}
    />
  )
}
