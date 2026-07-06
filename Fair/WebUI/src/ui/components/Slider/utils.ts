import { getVideoType, urlRegexMap } from "utils"
import { VideoType } from "types"

import { SliderItem } from "./types"

export function inferType(src: string): "image" | VideoType {
  const clean = src.split("#")[0]
  const type = getVideoType(clean)
  return type ?? "image"
}

export function inferPoster(src: string): string | undefined {
  const type = inferType(src)

  if (type === "youtube") {
    const match = src.match(urlRegexMap.youtube)
    const id = match ? match[1] : null
    return id ? `https://img.youtube.com/vi/${id}/hqdefault.jpg` : undefined
  }

  return undefined
}

export function getSlidePoster(item: SliderItem): string | undefined {
  const type = inferType(item.src)

  if (type === "image") {
    return item.src
  }

  if (type === "youtube") {
    return inferPoster(item.src) ?? item.poster
  }

  return item.poster ?? inferPoster(item.src)
}
