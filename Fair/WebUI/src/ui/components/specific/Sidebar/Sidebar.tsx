import { memo, MouseEvent, PropsWithChildren, useCallback, useLayoutEffect, useMemo } from "react"
import { twMerge } from "tailwind-merge"
import { last } from "lodash"
import { useRouteError } from "react-router-dom"

import { useStoreContext } from "app"
import { useGetPublicationDetails } from "entities"
import { useParams, useResolveStoreId } from "hooks"
import { PropsWithClassName } from "types"
import { buildCategoryTreeItems, buildRootCategoryItems } from "utils"

import { CurrentStore } from "./CurrentStoreButton"
import { CategoriesTree } from "./CategoriesTree"
import { SidebarSkeleton } from "./Sidebar.Skeleton"
import { useSidebarDrawer } from "./SidebarDrawer"

// Below `lg` the sidebar is an off-canvas drawer opened from the header, from `lg` it is a regular column.
const SIDEBAR_CLASSNAME = twMerge(
  "fixed inset-y-0 left-0 z-50 flex w-72 max-w-[85vw] -translate-x-full flex-col gap-8 overflow-y-auto bg-white p-4 shadow-md transition-[transform,visibility] duration-200 max-lg:invisible",
  "lg:static lg:z-auto lg:w-65 lg:max-w-65 lg:min-w-65 lg:shrink-0 lg:translate-x-0 lg:overflow-visible lg:bg-transparent lg:p-6 lg:shadow-none",
)

const SidebarShell = ({ className, children }: PropsWithChildren<PropsWithClassName>) => {
  const { isOpen, close } = useSidebarDrawer()

  const handleClick = useCallback(
    (e: MouseEvent<HTMLElement>) => {
      if ((e.target as HTMLElement).closest("a")) close()
    },
    [close],
  )

  return (
    <>
      <div
        className={twMerge(
          "fixed inset-0 z-40 bg-black/50 transition-opacity duration-200 lg:hidden",
          isOpen ? "opacity-100" : "pointer-events-none opacity-0",
        )}
        aria-hidden="true"
        onClick={close}
      />
      <aside
        className={twMerge(SIDEBAR_CLASSNAME, isOpen && "translate-x-0 max-lg:visible", className)}
        onClick={handleClick}
      >
        {children}
      </aside>
    </>
  )
}

export const Sidebar = memo(({ className }: PropsWithClassName) => {
  const { categoryId, publicationId } = useParams()
  const error = useRouteError()

  const storeId = useResolveStoreId()
  const { isLoading, isCategoriesLoading, store, categoriesTree, rootCategories } = useStoreContext()
  const { data: publication } = useGetPublicationDetails(publicationId)
  const { setHasSidebar } = useSidebarDrawer()

  const activeCategoryId = categoryId ?? last(publication?.path)?.id

  const items = useMemo(() => {
    if (categoriesTree?.length) {
      return buildCategoryTreeItems(categoriesTree, activeCategoryId)
    }

    return rootCategories?.length ? buildRootCategoryItems(rootCategories) : []
  }, [categoriesTree, rootCategories, activeCategoryId])

  const isStoreResolving = !storeId && !!(categoryId ?? publicationId)

  const isLoadingState = !error && (isLoading || isCategoriesLoading || isStoreResolving)
  const hasContent = !isLoadingState && !!store && !error

  useLayoutEffect(() => {
    setHasSidebar(isLoadingState || hasContent)
    return () => setHasSidebar(false)
  }, [hasContent, isLoadingState, setHasSidebar])

  if (isLoadingState) {
    return (
      <SidebarShell className={className}>
        <SidebarSkeleton />
      </SidebarShell>
    )
  }

  if (!store || error) {
    return null
  }

  return (
    <SidebarShell className={className}>
      <CurrentStore
        storeId={store.id}
        title={store.title}
        logoFileId={store.imageFileId}
        publishersCount={store.authorsIds.length}
      />
      {items.length > 0 && <CategoriesTree storeId={store.id} items={items} />}
    </SidebarShell>
  )
})
