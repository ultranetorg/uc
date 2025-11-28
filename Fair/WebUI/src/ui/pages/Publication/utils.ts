import { ComponentType } from "react"

import { ProductType } from "types"

import {
  BookPublicationContent,
  GamePublicationContent,
  MoviePublicationContent,
  MusicPublicationContent,
  SoftwarePublicationContent,
} from "./content"
import { ContentProps } from "./types"
import { DefaultPublicationContent } from "./content/DefaultPublicationContent"

export const getPublicationContentByType = (productType: ProductType): ComponentType<ContentProps> => {
  switch (productType) {
    case "book":
      return BookPublicationContent
    case "game":
      return GamePublicationContent
    case "movie":
      return MoviePublicationContent
    case "music":
      return MusicPublicationContent
    case "software":
      return SoftwarePublicationContent
  }

  return DefaultPublicationContent
}
