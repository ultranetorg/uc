import { useCallback, useMemo } from "react"
import { useTranslation } from "react-i18next"

export const useUserCommentMenuItems = (commentId: string) => {
  const { t } = useTranslation()

  const handleEdit = useCallback(() => alert("Edit comment" + commentId), [commentId])

  const menuItems = useMemo(
    () => [
      {
        onClick: handleEdit,
        label: t("common:edit"),
      },
    ],
    [handleEdit, t],
  )

  return { menuItems }
}
