import { ComponentType } from "react"

import { ProductType, Publication, PublicationExtended } from "types"
import {
  DefaultPublicationCard,
  BookPublicationCard,
  GamePublicationCard,
  MovePublicationCard,
  MusicPublicationCard,
  SoftwarePublicationCard,
} from "ui/components/specific"

export const getCardComponentForCategory = (
  productType: ProductType,
): ComponentType<{ siteId: string } & (Publication | PublicationExtended)> => {
  switch (productType) {
    case "book":
      return BookPublicationCard
    case "game":
      return GamePublicationCard
    case "movie":
      return MovePublicationCard
    case "music":
      return MusicPublicationCard
    case "software":
      return SoftwarePublicationCard
  }

  return DefaultPublicationCard
}
