import { Link, useParams } from "react-router-dom"
import { useMemo } from "react"

import { useGetCategories } from "entities"
import { buildCategoryTree } from "./utils"

export const CategoriesPage = () => {
  const { siteId } = useParams()

  const { isPending, data: categories } = useGetCategories(siteId, 2)

  const categoriesTree = useMemo(() => categories && buildCategoryTree(categories), [categories])

  if (isPending || !categoriesTree) {
    return <div>Loading...</div>
  }

  return (
    <div className="flex flex-wrap gap-4">
      {categoriesTree.map(x => (
        <div className="flex flex-col gap-2 rounded border border-gray-300 p-4" key={x.id}>
          <div className="font-bold">
            <Link to={`/${siteId}/c/${x.id}`} key={x.id}>
              {x.title}
            </Link>
          </div>
          {x.children.map(y => (
            <div className="flex flex-col gap-2" key={y.id}>
              <Link to={`/${siteId}/c/${y.id}`}>
                &nbsp;&nbsp;&nbsp;&nbsp;
                {y.title}
              </Link>
            </div>
          ))}
        </div>
      ))}
    </div>
  )
}
