export type BigCategoriesGridItem = {
  id: string
  title: string
  avatar?: string
}

export type BigCategoriesGridProps = {
  isLoading?: boolean
  siteId: string
  items?: BigCategoriesGridItem[]
}
