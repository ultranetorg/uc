import { useCallback, useMemo } from "react"
import { useTranslation } from "react-i18next"

export const useStoreDropdownMenuItems = (siteId: string) => {
  const { t } = useTranslation("storeDropdownMenu")

  const handleAvatarChange = useCallback(() => alert("Site avatar change " + siteId), [siteId])
  const handleNameChange = useCallback(() => alert("Site name change " + siteId), [siteId])
  const handleTextChange = useCallback(() => alert("Site text change " + siteId), [siteId])

  const menuItems = useMemo(
    () => [
      { label: t("common:proposals"), to: `/${siteId}/m` },
      { label: t("common:moderators"), to: `/${siteId}/m/m` },
      { label: t("common:publications"), to: `/${siteId}/m/c` },
      { label: t("common:publishers"), to: `/${siteId}/m/a` },
      { label: t("common:reviews"), to: `/${siteId}/m/r` },
      { label: t("common:users"), to: `/${siteId}/m/u` },
      { separator: true },
      {
        label: t("common:settings"),
        children: [
          { label: t("avatarChange"), onClick: handleAvatarChange },
          { label: t("nameChange"), onClick: handleNameChange },
          { label: t("textChange"), onClick: handleTextChange },
        ],
      },
      { separator: true },
      { label: t("common:about"), to: `/${siteId}/i` },
    ],
    [handleAvatarChange, handleNameChange, handleTextChange, siteId, t],
  )

  return { menuItems }
}
