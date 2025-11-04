import { Separator } from "./Separator"
import { TagLink } from "./TagLink"

export type TagLinksListItem = {
  label: string
  to: string
}

export type TagLinksListProps = {
  items?: TagLinksListItem[]
}

export const TagLinksList = ({ items }: TagLinksListProps) => (
  <div className="flex flex-wrap gap-3 whitespace-nowrap">
    {items &&
      items.map(({ label, to }, index) => (
        <div className="flex" key={to}>
          <TagLink label={label} to={to} />
          {index < items.length - 1 && <Separator className="ml-3" />}
        </div>
      ))}
  </div>
)
