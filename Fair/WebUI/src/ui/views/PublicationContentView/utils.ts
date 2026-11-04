import { ComponentType } from "react"

import { ProductType } from "types"

import {
  BookPublicationContent,
  DefaultPublicationContent,
  GameSoftwarePublicationContent,
  VideoPublicationContent,
  MusicPublicationContent,
} from "./content"
import { ContentProps } from "./types"

const publicationContentMap: Partial<Record<ProductType, ComponentType<ContentProps>>> = {
  book: BookPublicationContent,
  game: GameSoftwarePublicationContent,
  movie: VideoPublicationContent,
  music: MusicPublicationContent,
  software: GameSoftwarePublicationContent,
}

export const getPublicationContentByType = (productType: ProductType): ComponentType<ContentProps> => {
  return publicationContentMap[productType] ?? DefaultPublicationContent
}
