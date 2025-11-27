import { memo, ReactNode, useMemo, useRef, useState } from "react"
import { Link } from "react-router-dom"
import { ControlledMenu, FocusableItem, SubMenu, useClick } from "@szhsin/react-menu"

import { useRootContext } from "app"
import { Button } from "ui/components"
import { formatTitle } from "utils"
import { GridSvg } from "assets"

function LinkItem({ to, children, ...rest }: { to: string; children: ReactNode }) {
  return (
    <FocusableItem {...rest}>
      {({ ref, closeMenu }) => (
        <Link
          ref={ref}
          to={to}
          onClick={({ detail }) =>
            // Restore focus to menu button if activated by "Enter" key
            closeMenu(detail === 0 ? "Enter" : undefined)
          }
        >
          {children}
        </Link>
      )}
    </FocusableItem>
  )
}

export type CategoriesButtonProps = {
  siteId: string
}

export const CategoriesButton = memo(({ siteId }: CategoriesButtonProps) => {
  const ref = useRef(null)
  const [isOpen, setOpen] = useState(false)
  const anchorProps = useClick(isOpen, setOpen)

  const { categories, isCategoriesPending: isPending } = useRootContext()

  const menuItems = useMemo(() => {
    if (isPending || !categories) {
      return null
    }

    return categories!.map(x =>
      x.children.length > 0 ? (
        <SubMenu
          key={x.id}
          label={<Link to={`/${siteId}/c/${x.id}`}>{formatTitle(x.title)}</Link>}
          className="cursor-pointer"
          itemProps={{ onClick: () => setOpen(false) }}
        >
          {x.children.map(y => (
            <LinkItem key={y.id} to={`/${siteId}/c/${y.id}`}>
              {formatTitle(y.title)}
            </LinkItem>
          ))}
        </SubMenu>
      ) : (
        <LinkItem key={x.id} to={`/${siteId}/c/${x.id}`}>
          {formatTitle(x.title)}
        </LinkItem>
      ),
    )
  }, [siteId, categories, isPending])

  return (
    <>
      {/* @ts-expect-error aaa */}
      <Button
        className="gap-2"
        image={<GridSvg className="stroke-zinc-700" />}
        label={!isPending && categories ? <>Categories</> : <>⌛</>}
        ref={ref}
        {...anchorProps}
      />
      <ControlledMenu state={isOpen ? "open" : "closed"} anchorRef={ref} onClose={() => setOpen(false)}>
        {menuItems}
      </ControlledMenu>
    </>
    // <Menu
    //   menuButton={
    //     <MenuButton className="flex w-32 gap-2" ref={ref} {...anchorProps}>
    //       <GridSvg className="stroke-zinc-700" /> {!isPending && categories ? <>Categories</> : <>⌛</>}
    //     </MenuButton>
    //   }
    // >

    // </Menu>
  )
})
