import { useTranslation } from "react-i18next"
import { twMerge } from "tailwind-merge"

import { useResolveStoreId } from "hooks"
import { PropsWithClassName } from "types"

import { HeaderDropdownButton } from "../HeaderDropdownButton"

import { useGovernanceDropdownButtonMenuItems } from "./useGovernanceDropdownButtonMenuItems"

export const GovernanceDropdownButton = ({ className }: PropsWithClassName) => {
  const storeId = useResolveStoreId()
  const { t } = useTranslation()

  const menuItems = useGovernanceDropdownButtonMenuItems(storeId!)

  return (
    <HeaderDropdownButton
      className={twMerge(className, "first-letter:uppercase")}
      label={t("common:governance")}
      menuItems={menuItems}
    />
  )
}
