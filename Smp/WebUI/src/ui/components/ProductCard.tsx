export type ProductCardProps = {
  productName: string
  authorName: string
}

export const ProductCard = ({ productName, authorName }: ProductCardProps) => {
  return (
    <div className="flex flex-col border p-4">
      <div className="h-28 w-28 border bg-purple-300" />
      <span>{productName}</span>
      <span>{authorName}</span>
    </div>
  )
}
