import { memo, useCallback } from "react"
import { twMerge } from "tailwind-merge"
import { useTranslation } from "react-i18next"
import { FloatingPortal } from "@floating-ui/react"

import { useModerationContext } from "app"
import { useSubmenu } from "hooks"
import { PropsWithClassName, Site } from "types"
import { ChevronDownButton, SimpleMenu } from "ui/components"

import { LinkCounter } from "../LinkCounter"

import { useStoreDropdownMenuItems } from "./useStoreDropdownMenuItems"

type StoreDropdownButtonBaseProps = {
  site: Site
}

export type StoreDropdownButtonProps = PropsWithClassName & StoreDropdownButtonBaseProps

export const StoreDropdownButton = memo(({ className, site }: StoreDropdownButtonProps) => {
  const { isModerator } = useModerationContext()
  const { menuItems } = useStoreDropdownMenuItems(site.id)
  const { t } = useTranslation()

  const menu = useSubmenu({
    placement: "bottom-end",
  })

  const handleMenuClick = useCallback(() => menu.setOpen(false), [menu])

  if (!isModerator && !site.description) return null

  return isModerator ? (
    <>
      <div
        ref={menu.refs.setReference}
        className={twMerge(
          "flex cursor-pointer items-center text-2sm font-medium capitalize leading-6 text-gray-800 hover:font-semibold",
          className,
          menu.isOpen && "font-semibold",
        )}
        {...menu.getReferenceProps()}
      >
        {t("common:store")} <ChevronDownButton />
      </div>
      {menu.isOpen && (
        <FloatingPortal>
          <SimpleMenu
            ref={menu.refs.setFloating}
            items={menuItems}
            multiColumnMenu={false}
            style={menu.floatingStyles}
            onClick={handleMenuClick}
            {...menu.getFloatingProps()}
          />
        </FloatingPortal>
      )}
    </>
  ) : (
    <LinkCounter to={`/${site.id}/i`} className="w-[45px] capitalize">
      {t("about")}
    </LinkCounter>
  )
})
