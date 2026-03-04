import { useCallback, useMemo } from "react"
import { useTranslation } from "react-i18next"

export const useModeratorCategoryMenuItems = (categoryId: string) => {
  const { t } = useTranslation("moderatorCategoryMenu")

  const handleAvatarChange = useCallback(() => alert("Change category avatar" + categoryId), [categoryId])
  const handleMove = useCallback(() => alert("Move category" + categoryId), [categoryId])
  const handleRemove = useCallback(() => alert("Remove category" + categoryId), [categoryId])
  const handleCategoryCreate = useCallback(() => alert("Create category" + categoryId), [categoryId])
  const handleTypeChange = useCallback(() => alert("Category type change" + categoryId), [categoryId])

  const menuItems = useMemo(
    () => [
      {
        onClick: handleAvatarChange,
        label: t("moderatorCategoryMenu:avatarChange"),
      },
      {
        onClick: handleTypeChange,
        label: t("moderatorCategoryMenu:typeChange"),
      },
      {
        onClick: handleMove,
        label: t("moderatorCategoryMenu:move"),
      },
      {
        onClick: handleCategoryCreate,
        label: t("moderatorCategoryMenu:categoryCreate"),
      },
      {
        separator: true,
      },
      {
        onClick: handleRemove,
        label: t("moderatorCategoryMenu:remove"),
      },
    ],
    [handleAvatarChange, handleMove, handleRemove, handleCategoryCreate, handleTypeChange, t],
  )

  return { menuItems }
}
