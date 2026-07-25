export type BigCategoriesGridItem = {
  id: string
  title: string
  avatarId?: string
}

export type BigCategoriesGridProps = {
  isLoading?: boolean
  storeId: string
  items?: BigCategoriesGridItem[]
}
