import { uniq } from "lodash"

import { Description, DownloadSource, FieldValue, Release } from "types"
import { SoftwareDownload } from "ui/components/publication"
import { ensureHttp, getValue, isIpfsUri, isMagnetUri, isRdnLink, isValidUrl, nameEq } from "utils"

type BookFields = {
  author?: string
  publisher?: string
  isbn?: string
  publicationDate?: string
  genre?: string
  about?: string
  coverId?: string
}

export const buildBookFields = (fields?: FieldValue[]): BookFields => {
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

export function buildMovieFields(fields: FieldValue[] | undefined): MovieFieldMap {
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

export const getMaxDescription = (fields: FieldValue[] | undefined): string | undefined => {
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

// New
export const getDescriptions = (fields: FieldValue[]): Description[] | undefined => {
  const description = fields
    .filter(x => x.name.toLocaleLowerCase() === "description-maximal")
    .map<Partial<Description>>(x => ({
      language: getValue(x.children, "language")?.trim()?.toLocaleUpperCase(),
      text: getValue(x.children, "value")?.trim(),
    }))
    .filter(x => x.language && x.language.length > 0 && x.text && x.text.length > 0)
  const uniqDescrs = [...new Map(description.map(u => [u.language, u])).values()]
  return uniqDescrs.length > 0 ? (uniqDescrs as Description[]) : undefined
}

export const getTags = (fields: FieldValue[]): string[] | undefined => {
  const tags = fields
    .filter(x => x.name.toLocaleLowerCase() === "tags")
    .flatMap<string>(x => x.value as string)
    .join(" ")
    .split(/[,;\s]+/g)
    .map(x => x.trim())
    .filter(Boolean)
    .map(x => x.toLocaleLowerCase())
  const uniqTags = uniq(tags)
  return uniqTags.length > 0 ? uniqTags : undefined
}

export const getValueFrom = (fields: FieldValue[], name: string): string | undefined => {
  const value = fields.find(x => x.name.toLocaleLowerCase() === name)
  return value !== undefined ? value.value?.toString()?.trim() : undefined
}

export const getUrlFrom = (fields: FieldValue[], name: string): string | undefined => {
  const value = fields.find(x => x.name.toLocaleLowerCase() === name)
  if (value === undefined) return undefined
  const trimmed = (value.value as string | undefined)?.trim()
  return trimmed !== undefined && isValidUrl(trimmed) ? ensureHttp(trimmed) : undefined
}

export const getUiLanguages = (fields: FieldValue[]): string[] | undefined => {
  const values = fields.filter(x => x.name.toLocaleLowerCase() === "uilanguages")
  const langs = values
    .flatMap(x => x.children)
    .filter(x => x?.name?.toLocaleLowerCase() === "language")
    .map<string>(x => x?.value as string)
    .filter(Boolean)
    .map(x => x.toLocaleUpperCase())
  const uniqLangs = uniq(langs)
  return uniqLangs.length > 0 ? uniqLangs : undefined
}

export const normalizePlatformName = (raw: string): string => {
  const value = raw.trim().toLocaleLowerCase()

  if (value.includes("windows")) return "windows"
  if (value.includes("mac")) return "macos"
  if (value.includes("linux")) return "linux"

  return raw
}

export const getAllSupportedPlatforms = (releases: Release[]): string[] => {
  const order = ["windows", "macos", "linux"]
  const platforms = uniq(releases.map(x => x.requirements.platform.platform))
  return platforms.sort((a, b) => {
    const ia = order.indexOf(a)
    const ib = order.indexOf(b)
    if (ia === -1 && ib === -1) return 0
    if (ia === -1) return 1
    if (ib === -1) return -1
    return ia - ib
  })
}

const getDownloadSourceByLink = (link: string): DownloadSource | undefined => {
  if (isMagnetUri(link)) return "torrent"
  if (isIpfsUri(link)) return "ipfs"
  if (isRdnLink(link)) return "rdn"

  return undefined
}

export const getSoftwareDownloads = (releases: Release[], platform: string): SoftwareDownload[] | undefined => {
  const release = releases.find(x => x.requirements.platform.platform === platform)
  if (!release) return undefined
  const sources = release.distributive.sources.flatMap<SoftwareDownload>(x => {
    const source = getDownloadSourceByLink(x.uri)
    return source ? [{ link: x.uri, source }] : []
  })
  return sources.length > 0 ? sources : undefined
}
