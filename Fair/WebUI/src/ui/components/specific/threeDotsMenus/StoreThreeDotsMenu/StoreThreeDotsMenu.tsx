import { memo, useMemo } from "react"

import { useStoreRolesContext } from "app"
import { useResolveStoreId } from "hooks"
import { PropsWithClassName } from "types"
import { ThreeDotsMenu } from "ui/components"

import { useStoreThreeDotsMenu } from "./useStoreThreeDotsMenu"

export const StoreThreeDotsMenu = memo(({ className }: PropsWithClassName) => {
  const { isModerator } = useStoreRolesContext()
  const storeId = useResolveStoreId()

  const categoryCreationItem = useStoreThreeDotsMenu(storeId!)
  const menuItems = useMemo(() => [categoryCreationItem], [categoryCreationItem])

  if (!isModerator) {
    return null
  }

  return <ThreeDotsMenu className={className} items={menuItems} />
})
