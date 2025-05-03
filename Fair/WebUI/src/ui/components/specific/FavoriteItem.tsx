import { memo } from "react"

export type FavoriteItemProps = {
  title: string
}

export const FavoriteItem = memo(({ title }: FavoriteItemProps) => (
  <div className="flex items-center gap-3">
    <div className="h-10 w-10 rounded-lg bg-zinc-700 font-medium" />
    <span>{title}</span>
  </div>
))
