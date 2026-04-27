import { useCallback, useMemo } from "react"
import { useTranslation } from "react-i18next"

export const useModeratorPublicationMenuItems = (publicationId: string) => {
  const { t } = useTranslation("moderatorPublicationMenu")

  const handleRemovePublication = useCallback(
    () => alert("Remove publication useModeratorPublicationMenuItems" + publicationId),
    [publicationId],
  )

  const menuItems = useMemo(
    () => [
      {
        onClick: handleRemovePublication,
        label: t("removePublication"),
      },
    ],
    [handleRemovePublication, t],
  )

  return { menuItems }
}
