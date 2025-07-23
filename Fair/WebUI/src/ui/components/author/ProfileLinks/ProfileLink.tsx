import { memo, ReactNode } from "react"

export type ProfileLinkProps = {
  link: string
  text: string
  icon: ReactNode
}

export const ProfileLink = memo(({ link, text, icon }: ProfileLinkProps) => (
  <a
    href={link}
    target="_blank"
    title={text}
    rel="noopener noreferrer"
    className="box-border flex h-10 w-55 items-center gap-2 rounded border border-gray-300 bg-gray-100 px-3 py-2 text-2xs font-medium leading-4 hover:bg-gray-200"
  >
    {icon}
    <span className="overflow-hidden text-ellipsis whitespace-nowrap">{text}</span>
  </a>
))
