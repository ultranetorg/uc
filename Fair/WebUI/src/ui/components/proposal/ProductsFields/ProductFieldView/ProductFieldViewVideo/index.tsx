import { JSX, memo } from "react"
import { base64ToUtf8String } from "utils"
import { ProductFieldViewVideoYouTube } from "./ProductFieldViewVideoYouTube.tsx"
import { Link } from "react-router-dom"
import { ProductFieldViewVideoVkVideo } from "./ProductFieldViewVideoVkVideo.tsx"
import { ProductFieldViewVideoPlain } from "./ProductFieldViewVideoPlain.tsx"

const urlRegexMap = {
  plain: /\.(mp4|webm|ogg)$/i,
  youtube: /(?:https?:\/\/)?(?:www\.)?(?:youtube\.com\/watch\?v=|youtu\.be\/|youtube\.com\/shorts\/)([\w-]{11})/,
  vkVideo: /(?:https?:\/\/)?(?:www\.)?(?:vk\.com|vkvideo\.ru)\/.*video.*/i,
}

function getComponent(url: string): JSX.Element | null {
  switch (url) {
    case urlRegexMap.youtube.test(url) ? url : null: {
      return <ProductFieldViewVideoYouTube url={url} regex={urlRegexMap.youtube} />
    }
    case urlRegexMap.vkVideo.test(url) ? url : null: {
      return <ProductFieldViewVideoVkVideo url={url} />
    }
    case urlRegexMap.plain.test(url) ? url : null: {
      return <ProductFieldViewVideoPlain url={url} />
    }
  }

  return null;
}

export const ProductFieldViewVideo = memo(({ value }: { value: string }) => {
  const url = base64ToUtf8String(value)
  let component = getComponent(url);

  if (!component) {
    component = (
      <Link
        to={url}
        target="_blank"
        className="text-blue-600 underline transition-colors duration-150 hover:text-blue-800 hover:underline"
      >
        {url}
      </Link>
    )
  }

  return <div className="h-full">{component}</div>
})
