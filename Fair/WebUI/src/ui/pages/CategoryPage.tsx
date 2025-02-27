import { Link, useParams } from "react-router-dom"

import { useGetCategory } from "entities"

export const CategoryPage = () => {
  const { siteId, categoryId } = useParams()
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
      <span>
        PARENT ID:{" "}
        {category.parentId ? <Link to={`/${siteId}/c/${category.parentId}`}>{category.parentTitle}</Link> : ""}
      </span>
      <span>PARENT TITLE: {category.parentTitle}</span>

      {category.categories ? (
        <>
          <span className="text-black">CATEGORIES:</span>
          <ul>
            {category.categories.map(c => (
              <ol key={c.id}>
                <Link to={`/${siteId}/c/${c.id}`}>{c.title}</Link>
              </ol>
            ))}
          </ul>
        </>
      ) : (
        <>🚫 NO CATEGORIES</>
      )}

      {category.publications ? (
        <>
          <span className="text-black">PUBLICATIONS:</span>
          <ul>
            {category.publications.map(p => (
              <li key={p.id}>
                <Link to={`/${siteId}/p/${p.id}`}>{p.productTitle || p.id}</Link>
              </li>
            ))}
          </ul>
        </>
      ) : (
        <>🚫 NO PUBLICATIONS</>
      )}
    </div>
  )
}
