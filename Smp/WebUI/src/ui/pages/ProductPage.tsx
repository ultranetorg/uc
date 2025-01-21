import { useParams } from "react-router-dom"
import { useDocumentTitle } from "usehooks-ts"

import { useGetProduct } from "entities"

export const ProductPage = () => {
  const { productId } = useParams()
  useDocumentTitle(productId ? `Product - ${productId} | Ultranet Explorer` : "Product | Ultranet Explorer")

  const { isPending, data: product } = useGetProduct(productId)

  if (isPending) {
    return <div>Loading</div>
  }

  return (
    <div>
      <h1>ProductPage</h1>
      {JSON.stringify(product)}
    </div>
  )
}
