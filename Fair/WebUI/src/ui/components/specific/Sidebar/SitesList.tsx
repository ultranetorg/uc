import { memo } from "react"
import { Link } from "react-router-dom"

import { SiteBase } from "types"

import { Site } from "./components"

export type SitesListProps = {
  title: string
  items?: SiteBase[]
}

export const SitesList = memo(({ title, items }: SitesListProps) =>
  !items ? null : (
    <div className="flex flex-col gap-4">
      <span className="text-xs uppercase leading-3.75 tracking-tight-048 text-gray-500">{title}</span>
      {items.map(({ id, title }) => (
        <Link key={id} to={`/s/${id}`}>
          <Site title={title} />
        </Link>
      ))}
    </div>
  ),
)
