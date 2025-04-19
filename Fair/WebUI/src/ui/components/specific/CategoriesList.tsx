import { Link } from "react-router-dom"

import { Folder2Svg } from "assets"
import { CategoryBase } from "types"

import { CategoryCard } from "./CategoryCard"

export type CategoriesListProps = {
  siteId: string
  isPending: boolean
  categories: CategoryBase[]
}

export const CategoriesList = ({ siteId, isPending, categories }: CategoriesListProps) => (
  <div className="flex gap-8">
    {isPending ? (
      <div>⌛ LOADING</div>
    ) : categories.length === 0 ? (
      <div>🚫 NO CATEGORIES</div>
    ) : (
      <>
        {categories.map(x => (
          <Link key={x.id} to={`/${siteId}/c/${x.id}`}>
            <CategoryCard key={x.id} image={<Folder2Svg className="h-6 w-6 stroke-zinc-700" />} title={x.title} />
          </Link>
        ))}
      </>
    )}
  </div>
)
