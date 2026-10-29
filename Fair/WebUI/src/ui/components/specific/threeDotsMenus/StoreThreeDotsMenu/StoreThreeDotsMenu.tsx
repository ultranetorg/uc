import { memo } from "react"

import { useResolveStoreId } from "hooks"
import { PropsWithClassName } from "types"
import { ThreeDotsMenu } from "ui/components"

import { useStoreThreeDotsMenu } from "./useStoreThreeDotsMenu"

export const StoreThreeDotsMenu = memo(({ className }: PropsWithClassName) => {
  const storeId = useResolveStoreId()

  const menuItems = useStoreThreeDotsMenu(storeId!)

  return <ThreeDotsMenu className={className} items={menuItems} />
})
