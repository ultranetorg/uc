import { memo, useMemo } from "react"

import { useStoreRolesContext } from "app"
import { useResolveStoreId, useStoreChangeMenuItems } from "hooks"
import { PropsWithClassName } from "types"
import { ThreeDotsMenu } from "ui/components"

export const StoreMenu = memo(({ className }: PropsWithClassName) => {
  const { isModerator, isPublisher } = useStoreRolesContext()
  const storeId = useResolveStoreId()

  const storeItems = useStoreChangeMenuItems(storeId!, { moderators: isModerator, publishers: isPublisher })

  const menuItems = useMemo(() => [...storeItems], [storeItems])

  if ((!isModerator && !isPublisher) || menuItems.length === 0) {
    return null
  }

  return <ThreeDotsMenu className={className} items={menuItems} />
})
