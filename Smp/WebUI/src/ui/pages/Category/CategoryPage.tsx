import { Link, useParams } from "react-router-dom"

import { useGetCategory } from "entities"

export const CategoryPage = () => {
  const { categoryId } = useParams()
  const { data: category, isPending } = useGetCategory(categoryId)

  if (isPending || !category) {
    return <div>LOADING</div>
  }

  console.log(JSON.stringify(category))

  return (
    <div className="flex flex-col">
      <span>ID: {category.id}</span>
      <span>TITLE: {category.title}</span>
      <span>SITE ID: {category.siteId}</span>
      <span>PARENT ID: {category.parentId}</span>
      <span>PARENT TITLE: {category.parentTitle}</span>

      {category.categories && (
        <>
          <span className="text-black">CATEGORIES:</span>
          <ul>
            {category.categories?.map(c => (
              <ol key={c.id}>
                <Link to={`/c/${c.id}`}>{c.title}</Link>
              </ol>
            ))}
          </ul>
        </>
      )}

      {category.publications && (
        <>
          <span className="text-black">PUBLICATIONS:</span>
          <ul>
            {category.publications?.map(p => (
              <ol key={p.id}>
                <Link to={`/p/${p.id}`}>{p.title}</Link>
              </ol>
            ))}
          </ul>
        </>
      )}
    </div>
  )
}
