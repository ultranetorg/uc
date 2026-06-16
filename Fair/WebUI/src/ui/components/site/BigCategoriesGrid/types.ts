export type BigCategoriesGridItem = {
  id: string
  title: string
  avatarId?: string
}

export type BigCategoriesGridProps = {
  isLoading?: boolean
  siteId: string
  items?: BigCategoriesGridItem[]
}
