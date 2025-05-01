import { FavoriteItem } from "./FavoriteItem"

export type FavoritesListProps = {
  isPending: boolean
  favorites: { id: string; title: string }[]
}

export const FavoritesList = ({ isPending, favorites }: FavoritesListProps) => (
  <div className="flex flex-col gap-4">
    {isPending ? (
      <div>⌛ LOADING</div>
    ) : favorites.length === 0 ? (
      <div>🚫 NO FAVORITES</div>
    ) : (
      favorites.map(x => <FavoriteItem key={x.id} title={x.title} />)
    )}
  </div>
)
