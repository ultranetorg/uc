import { useParams } from "react-router-dom"
import { useDocumentTitle } from "usehooks-ts"

import { useGetProduct } from "entities"

import { Review } from "./Review"

export const ProductPage = () => {
  const { productId } = useParams()
  useDocumentTitle(productId ? `Product - ${productId} | Ultranet Explorer` : "Product | Ultranet Explorer")

  const { isPending, data: product } = useGetProduct(productId)

  if (isPending) {
    return <div>Loading</div>
  }

  return (
    <div className="flex flex-col gap-3">
      <div className="flex gap-4 text-black">
        <div className="w-1/2 border border-black px-4 py-3 text-center text-purple-500">Logo/Screenshot</div>
        <div className="flex w-1/2 flex-col border border-black px-4 py-3">
          <span>Publisher</span>
          <span>Publisher website link</span>
          <span>Category</span>
          <ul>
            <li>RDN Download link 1</li>
            <li>HTTP Download link 2</li>
            <li>Torrent Download link 3</li>
          </ul>
          <span>RDN Active users</span>
          <span>{JSON.stringify(product)}</span>
        </div>
      </div>
      <div className="border border-black px-4 py-3 text-black">Description</div>
      <div className="flex flex-col border border-black px-4 py-3 text-black">
        <span>Platform 1</span>
        <ul className="ml-4">
          <li>Requirements</li>
        </ul>
        <span>Platform 2</span>
        <ul className="ml-4">
          <li>Requirements</li>
        </ul>
        <span>Platform 3</span>
        <ul className="ml-4">
          <li>Requirements</li>
        </ul>
      </div>
      <div className="flex flex-col gap-3">
        <Review text="asdfasdfasdf asdfasdfasdf asdfasdfasdf 1" rating={3} userId="123" userName="Lex" />
        <Review text="asdfasdfasdf asdfasdfasdf asdfasdfasdf 2" rating={2} userId="124" userName="Luger" />
        <Review text="asdfasdfasdf asdfasdfasdf asdfasdfasdf 3" rating={4} userId="125" userName="Doinc" />
      </div>
    </div>
  )
}
