import { FieldValue } from "types"
import { SliderItem } from "ui/components/Slider/types"
import { buildFileUrl, ensureHttp, nameEq } from "utils"

export function buildMediaItems(fields: FieldValue[] | undefined): SliderItem[] {
  const arts = (fields ?? []).filter(x => nameEq(x.name, "art"))
  const items: { src: string; poster?: string; alt?: string }[] = []

  for (const art of arts) {
    const children = art.children ?? []

    const videos = (children.find(x => nameEq(x.name, "video"))?.children ?? [])
      .filter(x => nameEq(x.name, "uri") && x.value)
      .map(x => x.value)
    const screenshots = (children.find(x => nameEq(x.name, "screenshot"))?.children ?? [])
      .filter(x => nameEq(x.name, "id") && x.value)
      .map(x => x.value)

    for (const video of videos) {
      const videoUrl = ensureHttp(String(video))
      items.push({ src: videoUrl })
    }
    for (const screenshot of screenshots) {
      const screenshotUrl = buildFileUrl(String(screenshot))
      items.push({ src: screenshotUrl! })
    }
  }

  const unique = new Map<string, { src: string; poster?: string; alt?: string }>()
  for (const it of items) {
    if (!unique.has(it.src)) unique.set(it.src, it)
  }
  return Array.from(unique.values())
}
