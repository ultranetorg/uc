import { memo, useCallback, useMemo } from "react"
import { FloatingPortal } from "@floating-ui/react"
import { useTranslation } from "react-i18next"

import { SvgThreeDotsVertical } from "assets"
import { useScrollOrResize, useSubmenu } from "hooks"
import { SimpleMenu, SimpleMenuItem } from "ui/components"

export type ProposalCommentThreeDotsMenuProps = {
  onEdit: () => void
}

export const ProposalCommentThreeDotsMenu = memo(({ onEdit }: ProposalCommentThreeDotsMenuProps) => {
  const { t } = useTranslation("common")

  const menu = useSubmenu({ offset: 4, placement: "bottom-end" })
  useScrollOrResize(() => menu.setOpen(false), menu.isOpen)

  const handleEditClick = useCallback(() => {
    menu.setOpen(false)
    onEdit()
  }, [menu, onEdit])

  const handleMenuClick = useCallback(() => menu.setOpen(false), [menu])

  const menuItems = useMemo<SimpleMenuItem[]>(
    () => [{ label: t("edit"), onClick: handleEditClick }],
    [handleEditClick, t],
  )

  return (
    <>
      <div ref={menu.refs.setReference} {...menu.getReferenceProps()}>
        <SvgThreeDotsVertical className="cursor-pointer fill-gray-500 hover:fill-gray-800" />
      </div>
      {menu.isOpen && (
        <FloatingPortal>
          <SimpleMenu
            ref={menu.refs.setFloating}
            className="w-auto"
            items={menuItems}
            style={menu.floatingStyles}
            onClick={handleMenuClick}
            {...menu.getFloatingProps()}
          />
        </FloatingPortal>
      )}
    </>
  )
})
