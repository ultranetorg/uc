import { memo } from "react"
import { Link } from "react-router-dom"

import { CategoryPublications as CategoryPublicationsType } from "types"
import {
  // EBookPublicationCard,
  // GamePublicationCard,
  // MovePublicationCard,
  // MusicPublicationCard,
  PublicationsGrid,
  SoftwarePublicationCard,
} from "ui/components/specific"

import { CategoriesPublicationsListChildrenProps } from "./types"

type CategoryPublicationsListBaseProps = {
  siteId: string
}

export type CategoryPublicationsListProps = CategoryPublicationsType &
  CategoryPublicationsListBaseProps &
  CategoriesPublicationsListChildrenProps

export const CategoryPublicationsList = memo(
  ({ id, title, publications, siteId, seeAllLabel }: CategoryPublicationsListProps) => {
    return (
      <div className="flex flex-col gap-4">
        <div className="flex items-center justify-between text-gray-800">
          <span className="text-xl font-bold leading-6">{title}</span>
          <Link className="text-sm font-medium leading-4.25 hover:font-semibold" to={`/${siteId}/c/${id}`}>
            {seeAllLabel}
          </Link>
        </div>
        <PublicationsGrid
          siteId={siteId}
          isPending={false}
          publications={publications}
          CardComponent={SoftwarePublicationCard}
        />
      </div>
    )
  },
)
