import { formatTitle } from "utils"

export type CategoryCardProps = {
  image: JSX.Element
  title: string
}

export const CategoryCard = ({ image, title }: CategoryCardProps) => (
  <div
    className="flex items-center gap-2 rounded-lg border border-zinc-800 px-11 py-8 hover:bg-slate-300"
    title={title}
  >
    <div>{image}</div>
    <span>{formatTitle(title)}</span>
  </div>
)
