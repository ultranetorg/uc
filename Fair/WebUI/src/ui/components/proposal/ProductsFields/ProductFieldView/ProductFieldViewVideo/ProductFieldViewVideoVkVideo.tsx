import { memo } from "react"
import { useTranslation } from "react-i18next"
import { VideoFrame } from "ui/components/common"

function buildEmbedUrl(raw: string): string | null {
  try {
    const prefixed = raw.startsWith("http") ? raw : `https://${raw}`
    const u = new URL(prefixed)
    const host = u.hostname.toLowerCase()
    const path = u.pathname

    if (path.includes("video_ext.php") && (u.searchParams.get("oid") || u.searchParams.get("id"))) {
      return `${u.protocol}//${u.hostname}${u.pathname}${u.search}`
    }

    const match = path.match(/video-?([0-9]+)_([0-9]+)/i) || path.match(/video(-?\d+)_([0-9]+)/i)

    if (match) {
      const ownerRaw = match[1]
      const vid = match[2]

      const ownerNum = Number(ownerRaw)
      const oid = ownerNum > 0 ? `-${ownerNum}` : `${ownerNum}`

      const hash = u.searchParams.get("hash")
      const hostForEmbed = host.includes("vkvideo.ru") ? u.hostname : "vk.com"
      const base = `https://${hostForEmbed}/video_ext.php?oid=${encodeURIComponent(oid)}&id=${encodeURIComponent(vid)}`
      return hash ? `${base}&hash=${encodeURIComponent(hash)}` : base
    }

    const videosMatch = path.match(/videos?(-?\d+)\/(\d+)/i)
    if (videosMatch) {
      const ownerNum = Number(videosMatch[1])
      const oid = ownerNum > 0 ? `-${ownerNum}` : `${ownerNum}`
      const vid = videosMatch[2]
      return `https://vk.com/video_ext.php?oid=${encodeURIComponent(oid)}&id=${encodeURIComponent(vid)}`
    }

    return null
  } catch {
    return null
  }
}

export const ProductFieldViewVideoVkVideo = memo(({ url }: { url: string }) => {
  const { t } = useTranslation("common")

  const embedUrl = buildEmbedUrl(url)

  if (!embedUrl) {
    return <div> {t("invalidUrl")} </div>
  }

  return <VideoFrame src={embedUrl} title="VK video player" />
})
