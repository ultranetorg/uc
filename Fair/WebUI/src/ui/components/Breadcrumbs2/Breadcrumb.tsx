import { memo } from "react"
import { Link } from "react-router-dom"

import { formatTitle } from "utils"

import { BreadcrumbsItem2 } from "./types"

type Breadcrumb2Props = Extract<BreadcrumbsItem2, { title: string }>

export const Breadcrumb2 = memo(({ title, path }: Breadcrumb2Props) => {
  const formattedTitle = formatTitle(title)

  return path ? (
    <Link className="hover:font-medium" to={path} title={title}>
      {formattedTitle}
    </Link>
  ) : (
    <span className="text-gray-400" title={title}>
      {formattedTitle}
    </span>
  )
})
