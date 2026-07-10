import { memo, ReactNode } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

type ProfileLinkBaseProps = {
  link: string
  text: string
  icon: ReactNode
}

export type ProfileLinkProps = PropsWithClassName & ProfileLinkBaseProps

export const ProfileLink = memo(({ className, link, text, icon }: ProfileLinkProps) => (
  <a
    href={link}
    target="_blank"
    title={text}
    rel="noopener noreferrer"
    className={twMerge(
      "box-border flex h-10 w-55 items-center gap-2 rounded border border-gray-300 bg-gray-100 px-3 py-2 text-2xs font-medium leading-4 hover:bg-gray-200",
      className,
    )}
  >
    {icon}
    <span className="truncate">{text}</span>
  </a>
))
