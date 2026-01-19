import { ProductField } from "types"
import { getValue, nameEq } from "utils"

type BookFields = {
  author?: string
  publisher?: string
  isbn?: string
  publicationDate?: string
  genre?: string
  about?: string
  coverId?: string
}

export const buildBookFields = (fields?: ProductField[]): BookFields => {
  if (!fields?.length) return {}

  return {
    author: getValue(fields, "author"),
    publisher: getValue(fields, "publisher"),
    isbn: getValue(fields, "isbn"),
    publicationDate: getValue(fields, "publication-date"),
    genre: getValue(fields, "genre"),
    about: getValue(fields, "about"),
    coverId: getValue(fields, "cover-id"),
  }
}

type MovieFieldMap = {
  director?: string
  writer?: string
  stars?: string
  time?: string
  releaseDate?: string
  countries?: string
  genre?: string
  quality?: string
  translation?: string
  imdb?: string
  posterId?: string
  about?: string
}

export function buildMovieFields(fields: ProductField[] | undefined): MovieFieldMap {
  if (!fields?.length) return {}

  return {
    director: getValue(fields, "director"),
    writer: getValue(fields, "writer"),
    stars: getValue(fields, "stars"),
    time: getValue(fields, "time"),
    releaseDate: getValue(fields, "release-date"),
    countries: getValue(fields, "countries"),
    genre: getValue(fields, "genre"),
    quality: getValue(fields, "quality"),
    translation: getValue(fields, "translation"),
    imdb: getValue(fields, "imdb"),
    posterId: getValue(fields, "cover-id"),
    about: getValue(fields, "about"),
  }
}

export const getMaxDescription = (fields: ProductField[] | undefined): string | undefined => {
  if (!fields?.length) return undefined

  const nodes = fields.filter(f => nameEq(f.name, "description-maximal"))
  if (!nodes.length) return undefined

  // Prefer English description if present
  for (const node of nodes) {
    const children = node.children ?? []
    const lang = getValue(children, "language")
    const text = getValue(children, "value") ?? getValue(children, "description") ?? undefined

    if ((lang ?? "").toLowerCase() === "en" && text) {
      return text
    }
  }

  // Fallback: first node with any text
  for (const node of nodes) {
    const children = node.children ?? []
    const text = getValue(children, "value") ?? getValue(children, "description") ?? undefined
    if (text) return text
  }

  return undefined
}
