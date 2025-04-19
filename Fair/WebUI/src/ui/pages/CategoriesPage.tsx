import { useParams } from "react-router-dom"
import { usePagePagination } from "./hooks"
import { useGetCategories } from "entities"

export const CategoriesPage = () => {
  const { siteId } = useParams()

  const { page, pageSize } = usePagePagination()

  const { isPending, data: categories } = useGetCategories(siteId, page, pageSize)

  if (isPending || !categories) {
    return <div>Loading...</div>
  }

  return <div>{JSON.stringify(categories)}</div>
}
