import { memo, useMemo } from "react"

import { useStoreRolesContext } from "app"
import { useCategoryCreationMenuItem, useResolveStoreId } from "hooks"
import { PropsWithClassName } from "types"
import { ThreeDotsMenu } from "ui/components"

export const ModeratorStoreMenu = memo(({ className }: PropsWithClassName) => {
  const { isModerator } = useStoreRolesContext()
  const storeId = useResolveStoreId()

  const categoryCreationItem = useCategoryCreationMenuItem(storeId!)
  const menuItems = useMemo(() => [categoryCreationItem], [categoryCreationItem])

  if (!isModerator) {
    return null
  }

  return <ThreeDotsMenu className={className} items={menuItems} />
})
