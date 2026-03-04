import { useCallback, useMemo } from "react"
import { useTranslation } from "react-i18next"

export const useLogoDropdownMenuItems = (siteId: string) => {
  const { t } = useTranslation("logoDropdownMenu")

  const handleAvatarChange = useCallback(() => alert("Site avatar change " + siteId), [siteId])
  const handleNameChange = useCallback(() => alert("Site name change " + siteId), [siteId])
  const handleTextChange = useCallback(() => alert("Site text change " + siteId), [siteId])

  const menuItems = useMemo(
    () => [
      {
        onClick: handleAvatarChange,
        label: t("avatarChange"),
      },
      {
        onClick: handleNameChange,
        label: t("nameChange"),
      },
      {
        onClick: handleTextChange,
        label: t("textChange"),
      },
    ],
    [handleAvatarChange, handleNameChange, handleTextChange, t],
  )

  return { menuItems }
}
