import { memo } from "react"
import { Link } from "react-router-dom"

import { CategoryPublications as CategoryPublicationsType } from "types"

import { PublicationsList } from "./PublicationsList"

type CategoryPublicationsListBaseProps = {
  siteId: string
}

export type CategoryPublicationsListProps = CategoryPublicationsType & CategoryPublicationsListBaseProps

export const CategoryPublicationsList = memo(({ id, title, publications, siteId }: CategoryPublicationsListProps) => {
  return (
    <div className="flex flex-col gap-4">
      <div>
        <h3>
          <Link to={`/${siteId}/c/${id}`}>{title}</Link>
        </h3>
      </div>
      <PublicationsList siteId={siteId} isPending={false} publications={publications} />
    </div>
  )
})
