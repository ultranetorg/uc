import { ReactElement } from "react"

export type BigCategoryCardProps = {
  icon: ReactElement
  title: string
}

export const BigCategoryCard = ({ icon, title }: BigCategoryCardProps) => (
  <div
    className="flex w-full flex-col items-center gap-2 rounded-lg bg-gray-100 py-5.25 hover:bg-gray-200"
    title={title}
  >
    {icon}
    <span className="w-40 overflow-hidden text-ellipsis whitespace-nowrap text-center text-2sm leading-4.5 text-gray-800">
      {title}
    </span>
  </div>
)
