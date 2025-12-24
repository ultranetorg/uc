import { memo, useCallback, useMemo, useState } from "react"
import { FloatingPortal } from "@floating-ui/react"
import { useTranslation } from "react-i18next"
import { twMerge } from "tailwind-merge"

import { SvgThreeDotsSm } from "assets"
import { useScrollOrResize, useSubmenu } from "hooks"
import { SimpleMenu, TextModal } from "ui/components"
import { PropsWithClassName } from "types"
import { useSiteContext } from "app"

type ModeratorOptionsMenuBaseProps = {
  publicationId: string
}

export type ModeratorOptionsMenuProps = PropsWithClassName & ModeratorOptionsMenuBaseProps

export const ModeratorOptionsMenu = memo(({ className, publicationId }: ModeratorOptionsMenuProps) => {
  const { t } = useTranslation("moderatorOptionsMenu")
  const { t: tCommon } = useTranslation("common")
  const { isModerator } = useSiteContext()

  const menu = useSubmenu({ placement: "bottom-end", offset: 4 })
  const [isRemoveModalOpen, setRemoveModalOpen] = useState(false)

  useScrollOrResize(() => menu.setOpen(false), menu.isOpen)

  const handleRemovePublication = useCallback(() => {
    menu.setOpen(false)
    setRemoveModalOpen(true)
  }, [menu])

  const handleConfirmRemove = useCallback(() => {
    setRemoveModalOpen(false)
    alert("Remove publication " + publicationId)
  }, [publicationId])

  const menuItems = useMemo(
    () => [
      {
        onClick: handleRemovePublication,
        label: t("removePublication"),
      },
    ],
    [handleRemovePublication, t],
  )

  const handleMenuClick = useCallback(() => menu.setOpen(false), [menu])

  if (!isModerator) {
    return null
  }

  return (
    <>
      <div
        className={twMerge(
          "flex cursor-pointer items-center gap-2 rounded border border-gray-300 bg-gray-100 px-3 py-2 hover:border-gray-400",
          className,
        )}
        ref={menu.refs.setReference}
        {...menu.getReferenceProps()}
        onClick={() => menu.setOpen(!menu.isOpen)}
      >
        <span className="text-2xs font-medium leading-4">{t("label")}</span>
        <SvgThreeDotsSm className="size-6 fill-gray-800" />
      </div>
      {menu.isOpen && (
        <FloatingPortal>
          <SimpleMenu
            ref={menu.refs.setFloating}
            items={menuItems}
            style={menu.floatingStyles}
            onClick={handleMenuClick}
            {...menu.getFloatingProps()}
          />
        </FloatingPortal>
      )}
      {isRemoveModalOpen && (
        <TextModal
          title={t("removePublication")}
          text={
            "Are you sure you want to remove the publication?\n\nAfter rejection, the review will be stored in the moderation section."
          }
          confirmLabel={tCommon("remove")}
          cancelLabel={tCommon("cancel")}
          onConfirm={handleConfirmRemove}
          onCancel={() => setRemoveModalOpen(false)}
        />
      )}
    </>
  )
})
