import { memo } from "react"

export interface VideoFrameProps {
  title: string
  src: string
}

export const VideoFrame = memo(({ src, title }: VideoFrameProps) => (
  <iframe
    width="100%"
    height="100%"
    src={src}
    title={title}
    frameBorder="0"
    allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
    allowFullScreen
  />
))
