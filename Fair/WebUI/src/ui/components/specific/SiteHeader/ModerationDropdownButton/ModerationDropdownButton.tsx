import { useTranslation } from "react-i18next"
import { twMerge } from "tailwind-merge"

import { useParams } from "hooks"
import { PropsWithClassName } from "types"

import { HeaderDropdownButton } from "../HeaderDropdownButton"

import { useModerationDropdownButtonMenuItems } from "./useModerationDropdownButtonMenuItems"

export const ModerationDropdownButton = ({ className }: PropsWithClassName) => {
  const { siteId } = useParams()
  const { t } = useTranslation()

  const menuItems = useModerationDropdownButtonMenuItems(siteId!)

  return (
    <HeaderDropdownButton
      className={twMerge(className, "first-letter:uppercase")}
      label={t("common:moderation")}
      menuItems={menuItems}
    />
  )
}
