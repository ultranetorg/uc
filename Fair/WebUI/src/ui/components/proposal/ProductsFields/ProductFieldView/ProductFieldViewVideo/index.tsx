import { memo } from "react"
import { base64ToUtf8String } from "utils"
import { ProductFieldViewVideoYouTube } from "./ProductFieldViewVideoYouTube.tsx"
import { ProductFieldViewVideoVkVideo } from "./ProductFieldViewVideoVkVideo.tsx"
import { ProductFieldViewVideoPlain } from "./ProductFieldViewVideoPlain.tsx"
import { ProductFieldViewProps } from "../product-field-view-props.ts"

const urlRegexMap = {
  plain: /\.(mp4|webm|ogg)$/i,
  youtube: /(?:https?:\/\/)?(?:www\.)?(?:youtube\.com\/watch\?v=|youtu\.be\/|youtube\.com\/shorts\/)([\w-]{11})/,
  vkVideo: /(?:https?:\/\/)?(?:www\.)?(?:vk\.com|vkvideo\.ru)\/.*video.*/i,
}

function getComponent(rawUrl: string | undefined): React.ReactNode {
  if (!rawUrl) return null

  const url = rawUrl.trim()

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
      <div className="w-80 h-48 overflow-hidden rounded-md border bg-black relative">{children}</div>
      {label ? <div className="mt-1 text-xs text-gray-500">{label}</div> : null}
    </div>
  )
}

export const ProductFieldViewVideo = memo(({ value, oldValue, status }: ProductFieldViewProps) => {
  const newUrl = base64ToUtf8String(value)
  const oldUrl = oldValue ? base64ToUtf8String(oldValue) : undefined

  const newPreview = getComponent(newUrl)
  const oldPreview = getComponent(oldUrl)

  switch (status) {
    case "added":
      return (
        <PreviewBox label="Added">
          <div className="w-full h-full p-1">
            <div className="w-full h-full border-2 border-green-500 rounded-md overflow-hidden bg-black">{newPreview}</div>
          </div>
        </PreviewBox>
      )

    case "removed":
      return (
        <PreviewBox label="Removed">
          <div className="w-full h-full relative">
            <div className="w-full h-full border-2 border-red-500 rounded-md overflow-hidden bg-black opacity-90">{oldPreview}</div>
            <div className="absolute inset-0 flex items-center justify-center pointer-events-none">
              <div className="bg-red-600/30 text-white text-sm px-2 py-1 rounded">Removed</div>
            </div>
          </div>
        </PreviewBox>
      )

    case "changed":
      return (
        <div className="flex gap-4 items-start">
          <PreviewBox label="Old">
            <div className="w-full h-full border-2 border-red-500 rounded-md overflow-hidden bg-black opacity-90">{oldPreview}</div>
          </PreviewBox>

          <PreviewBox label="New">
            <div className="w-full h-full border-2 border-green-500 rounded-md overflow-hidden bg-black">{newPreview}</div>
          </PreviewBox>
        </div>
      )

    default:
      return (
        <div className="w-full h-full">
          <div className="w-full h-full overflow-hidden rounded-md bg-black">{newPreview}</div>
        </div>
      )
  }
})
