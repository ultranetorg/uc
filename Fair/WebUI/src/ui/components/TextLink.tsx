import { Link as RouterLink, LinkProps } from "react-router-dom"
import { twMerge } from "tailwind-merge"

import { SvgLinkExternal, SvgLinkInternal } from "assets"
import { PropsWithClassName } from "types"

type LinkBaseProps = {
  children?: string
  external?: boolean
  showIcon?: boolean
}

type TextLinkProps = PropsWithClassName<LinkBaseProps> & LinkProps

export const TextLink = (props: TextLinkProps) => {
  const { children, className, external, showIcon, ...rest } = props
  return (
    <RouterLink
      className={twMerge(
        "group flex items-center gap-2 text-[#3DC1F2] no-underline hover:no-underline",
        !showIcon ? "block overflow-hidden text-ellipsis whitespace-nowrap" : "",
        className,
      )}
      {...rest}
    >
      {children}
      {showIcon &&
        (!external ? (
          <SvgLinkInternal className="fill-[##5E5E60] group-hover:fill-white" />
        ) : (
          <SvgLinkExternal className="fill-[##5E5E60] group-hover:fill-white" />
        ))}
    </RouterLink>
  )
}
