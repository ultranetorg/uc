import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { Link } from "react-router-dom"
import avatarFallback from "assets/fallback/author-8.png"
import { ImageFallback } from "ui/components/ImageFallback"
import { buildFileUrl } from "utils"
import { ButtonPrimary } from "ui/components"

export type AuthorImageTitleProps = {
  title: string
  authorFileId?: string
}

export const AuthorImageTitle = memo(({ title, authorFileId }: AuthorImageTitleProps) => (
  <div className="flex items-center gap-2">
    <div className="size-8 overflow-hidden rounded-full">
      <ImageFallback src={buildFileUrl(authorFileId)} fallbackSrc={avatarFallback} className="size-full object-cover" />
    </div>
    <span
      className={twMerge(
        "cursor-pointer overflow-hidden text-ellipsis whitespace-nowrap text-2sm font-medium leading-5 text-gray-800 hover:font-semibold",
      )}
    >
      {title}
    </span>
  </div>
))

export type DownloadLinksProps = {
  links: string[]
  label: string
}

export const DownloadLinks = memo(({ links, label }: DownloadLinksProps) => {
  if (links.length === 0) {
    return null
  }

  if (links.length === 1) {
    return (
      <Link to={links[0]} target="_blank">
        <ButtonPrimary className="w-full" label={label} />
      </Link>
    )
  }

  return links.map((x, i) => (
    <Link key={x + i} to={x} target="_blank">
      <ButtonPrimary className="w-full" label={`${label} ${i}`} />
    </Link>
  ))
})
