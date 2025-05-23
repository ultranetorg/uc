import { memo } from "react"
import { Link } from "react-router-dom"

import { SiteBase } from "types"

import { SidebarSite } from "./SidebarSite"

export type SidebarSitesListProps = {
  title: string
  items?: SiteBase[]
}

export const SidebarSitesList = memo(({ title, items }: SidebarSitesListProps) =>
  !items ? null : (
    <div className="flex flex-col gap-4">
      <span className="leading-3.75 tracking-tight-048 text-xs uppercase text-gray-500">{title}</span>
      {items.map(({ id, title }) => (
        <Link key={id} to={`/s/${id}`}>
          <SidebarSite title={title} />
        </Link>
      ))}
    </div>
  ),
)
