import { FieldValue } from "types"
import { buildFileUrl, ensureHttp, nameEq } from "utils"

export function buildMediaItems(fields: FieldValue[] | undefined) {
  const arts = (fields ?? []).filter(x => nameEq(x.name, "art"))
  const items: { src: string; poster?: string; alt?: string }[] = []

  for (const art of arts) {
    const children = art.children ?? []

    const screenshotChildren = children.find(c => nameEq(c.name, "screenshot"))?.children ?? []
    const videoChildren = children.find(c => nameEq(c.name, "video"))?.children ?? []

    const screenshotId = screenshotChildren.find(c => nameEq(c.name, "id"))?.value as string | undefined
    const screenshotUrl = screenshotId ? buildFileUrl(screenshotId) : undefined

    const videoUrlRaw = (videoChildren.find(c => nameEq(c.name, "uri"))?.value as string | undefined) ?? undefined
    const videoUrl = videoUrlRaw ? ensureHttp(String(videoUrlRaw)) : undefined

    if (videoUrl) {
      items.push({ src: videoUrl, poster: screenshotUrl })
    }

    if (screenshotUrl) {
      items.push({ src: screenshotUrl })
    }
  }

  const unique = new Map<string, { src: string; poster?: string; alt?: string }>()
  for (const it of items) {
    if (!unique.has(it.src)) unique.set(it.src, it)
  }
  return Array.from(unique.values())
}
