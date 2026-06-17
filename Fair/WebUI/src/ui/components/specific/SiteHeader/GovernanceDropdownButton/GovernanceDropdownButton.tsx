import { useTranslation } from "react-i18next"
import { twMerge } from "tailwind-merge"

import { useParams } from "react-router-dom"
import { PropsWithClassName } from "types"

import { HeaderDropdownButton } from "../HeaderDropdownButton"

import { useGovernanceDropdownButtonMenuItems } from "./useGovernanceDropdownButtonMenuItems"

export const GovernanceDropdownButton = ({ className }: PropsWithClassName) => {
  const { siteId } = useParams()
  const { t } = useTranslation()

  const menuItems = useGovernanceDropdownButtonMenuItems(siteId!)

  return (
    <HeaderDropdownButton
      className={twMerge(className, "first-letter:uppercase")}
      label={t("common:governance")}
      menuItems={menuItems}
    />
  )
}
