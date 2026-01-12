import { memo } from "react"

import { VideoViewPlain, VideoViewVk, VideoViewYouTube } from "ui/components/VideoView"
import { getVideoType } from "utils"
import { ProductFieldViewProp } from "./types"

function getComponent(rawUrl: unknown | null | undefined): React.ReactNode {
  if (!rawUrl) return null

  const url = (rawUrl as string).trim()
  const videoType = getVideoType(url)

  if (videoType === "youtube") {
    return <VideoViewYouTube url={url} />
  }

  if (videoType === "vkVideo") {
    return <VideoViewVk url={url} />
  }

  if (videoType === "video") {
    return <VideoViewPlain url={url} className="size-full bg-black" controls={true} />
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
          <div className="size-full p-1">
            <div className="size-full overflow-hidden rounded-md border-2 border-green-500 bg-black">{newPreview}</div>
          </div>
        </PreviewBox>
      )

    case "removed":
      return (
        <PreviewBox label="Removed">
          <div className="relative size-full">
            <div className="size-full overflow-hidden rounded-md border-2 border-red-500 bg-black opacity-90">
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
            <div className="size-full overflow-hidden rounded-md border-2 border-red-500 bg-black opacity-90">
              {oldPreview}
            </div>
          </PreviewBox>

          <PreviewBox label="New">
            <div className="size-full overflow-hidden rounded-md border-2 border-green-500 bg-black">{newPreview}</div>
          </PreviewBox>
        </div>
      )

    default:
      return (
        <div className="size-full">
          <div className="size-full overflow-hidden rounded-md bg-black">{newPreview}</div>
        </div>
      )
  }
})
