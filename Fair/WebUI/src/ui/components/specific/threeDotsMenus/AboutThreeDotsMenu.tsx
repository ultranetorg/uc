import { memo } from "react"

import { useStoreRolesContext } from "app"
import { useResolveStoreId, useStoreChangeMenuItems } from "hooks"
import { PropsWithClassName } from "types"
import { ThreeDotsMenu } from "ui/components"

export const AboutThreeDotsMenu = memo(({ className }: PropsWithClassName) => {
  const { isModerator, isPublisher } = useStoreRolesContext()
  const storeId = useResolveStoreId()

  const menuItems = useStoreChangeMenuItems(storeId!, { isModerator, isPublisher })

  return <ThreeDotsMenu className={className} items={menuItems} />
})
