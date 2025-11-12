import { memo } from "react"

import { ProductFieldViewVideoYouTube } from "./ProductFieldViewVideoYouTube"
import { ProductFieldViewVideoVkVideo } from "./ProductFieldViewVideoVkVideo"
import { ProductFieldViewVideoPlain } from "./ProductFieldViewVideoPlain"
import { ProductFieldViewProp } from "../types"

const urlRegexMap = {
  plain: /\.(mp4|webm|ogg)$/i,
  youtube: /(?:https?:\/\/)?(?:www\.)?(?:youtube\.com\/watch\?v=|youtu\.be\/|youtube\.com\/shorts\/)([\w-]{11})/,
  vkVideo: /(?:https?:\/\/)?(?:www\.)?(?:vk\.com|vkvideo\.ru)\/.*video.*/i,
}

function getComponent(rawUrl: unknown | null): React.ReactNode {
  if (!rawUrl) return null

  const url = (rawUrl as string).trim()

  if (urlRegexMap.youtube.test(url)) {
    return <ProductFieldViewVideoYouTube url={url} regex={urlRegexMap.youtube} />
  }

  if (urlRegexMap.vkVideo.test(url)) {
    return <ProductFieldViewVideoVkVideo url={url} />
  }

  if (urlRegexMap.plain.test(url)) {
    return <ProductFieldViewVideoPlain url={url} />
  }

  // fallback: plain link — external anchor
  return (
    <a
      href={url.startsWith("http") ? url : `https://${url}`}
      target="_blank"
      rel="noopener noreferrer"
      className="text-blue-600 underline transition-colors duration-150 hover:text-blue-800 hover:underline"
    >
      {url}
    </a>
  )
}

function PreviewBox({ children, label }: { children: React.ReactNode; label?: string }) {
  return (
    <div className="flex flex-col items-center text-sm">
      <div className="relative h-48 w-80 overflow-hidden rounded-md border bg-black">{children}</div>
      {label ? <div className="mt-1 text-xs text-gray-500">{label}</div> : null}
    </div>
  )
}

export const ProductFieldViewVideo = memo(({ value, oldValue, status }: ProductFieldViewProp) => {
  const newPreview = getComponent(value)
  const oldPreview = getComponent(oldValue)

  switch (status) {
    case "added":
      return (
        <PreviewBox label="Added">
          <div className="h-full w-full p-1">
            <div className="h-full w-full overflow-hidden rounded-md border-2 border-green-500 bg-black">
              {newPreview}
            </div>
          </div>
        </PreviewBox>
      )

    case "removed":
      return (
        <PreviewBox label="Removed">
          <div className="relative h-full w-full">
            <div className="h-full w-full overflow-hidden rounded-md border-2 border-red-500 bg-black opacity-90">
              {oldPreview}
            </div>
            <div className="pointer-events-none absolute inset-0 flex items-center justify-center">
              <div className="rounded bg-red-600/30 px-2 py-1 text-sm text-white">Removed</div>
            </div>
          </div>
        </PreviewBox>
      )

    case "changed":
      return (
        <div className="flex items-start gap-4">
          <PreviewBox label="Old">
            <div className="h-full w-full overflow-hidden rounded-md border-2 border-red-500 bg-black opacity-90">
              {oldPreview}
            </div>
          </PreviewBox>

          <PreviewBox label="New">
            <div className="h-full w-full overflow-hidden rounded-md border-2 border-green-500 bg-black">
              {newPreview}
            </div>
          </PreviewBox>
        </div>
      )

    default:
      return (
        <div className="h-full w-full">
          <div className="h-full w-full overflow-hidden rounded-md bg-black">{newPreview}</div>
        </div>
      )
  }
})
