export type ProductsGridEmptyProps = {
  message: string
}

export const ProductsGridEmpty = ({ message }: ProductsGridEmptyProps) => (
  <span className="py-6 text-lg leading-5 text-gray-800">{message}</span>
)
