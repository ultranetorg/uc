import { Link } from "react-router-dom"
import { Publication, PublicationSearch } from "types"

type PublicationBaseProps = {
  siteId: string
}

export type PublicationCardProps = PublicationBaseProps &
  Publication &
  Partial<Pick<PublicationSearch, "authorId" | "authorTitle" | "categoryId" | "categoryTitle">>

export const PublicationCard = (props: PublicationCardProps) => {
  return (
    <div className="relative flex flex-col items-center gap-4 rounded-lg border border-zinc-700 px-2 py-4">
      <div className="h-14 w-14 rounded-2xl border bg-zinc-700" />
      <div className="flex w-60 flex-col gap-1 text-center">
        <span className="overflow-hidden text-ellipsis whitespace-nowrap text-sm font-semibold">{props.title}</span>
        <span className="overflow-hidden text-ellipsis whitespace-nowrap text-xs">
          {props.supportedOSes.join(" | ")}
        </span>
        {props.categoryId && (
          <span className="overflow-hidden text-ellipsis whitespace-nowrap text-xs">
            {<Link to={`/${props.siteId}/c/${props.categoryId}`}>{props.categoryTitle}</Link>}
          </span>
        )}
        {props.authorId && (
          <span className="overflow-hidden text-ellipsis whitespace-nowrap text-xs">
            {<Link to={`/${props.siteId}/a/${props.authorId}`}>{props.authorTitle}</Link>}
          </span>
        )}
        <span className="absolute right-3 top-3">{props.averageRating.toFixed(1)} ⭐</span>
      </div>
    </div>
  )
}
