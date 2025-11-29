import { ComponentType } from "react"

import { ProductType } from "types"

import {
  BookPublicationContent,
  DefaultPublicationContent,
  GamePublicationContent,
  MoviePublicationContent,
  MusicPublicationContent,
  SoftwarePublicationContent,
} from "./content"
import { ContentProps } from "./types"

const publicationContentMap: Partial<Record<ProductType, ComponentType<ContentProps>>> = {
  book: BookPublicationContent,
  game: GamePublicationContent,
  movie: MoviePublicationContent,
  music: MusicPublicationContent,
  software: SoftwarePublicationContent,
}

export const getPublicationContentByType = (productType: ProductType): ComponentType<ContentProps> => {
  return publicationContentMap[productType] ?? DefaultPublicationContent
}
