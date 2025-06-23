import { Publication, PublicationExtended } from "types"

type PublicationBaseProps = {
  siteId: string
}

export type PublicationCardProps = PublicationBaseProps &
  Publication &
  Partial<Pick<PublicationExtended, "authorId" | "authorTitle" | "categoryId" | "categoryTitle">>
