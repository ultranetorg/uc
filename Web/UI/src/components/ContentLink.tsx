import { memo } from "react"
import { Link as RouterLink, LinkProps } from "react-router-dom"
import { twMerge } from "tailwind-merge"

import { SvgLinkExternal, SvgLinkInternal } from "assets"
import { PropsWithClassName } from "types"

type ContentLinkBaseProps = {
  children?: string
  external?: boolean
  showIcon?: boolean
}

type ContentLinkProps = PropsWithClassName & ContentLinkBaseProps & LinkProps

export const ContentLink = memo((props: ContentLinkProps) => {
  const { children, className, external, showIcon, ...rest } = props
  return (
    <RouterLink
      className={twMerge(
        "group flex items-center gap-2 text-cyan-500 no-underline hover:no-underline",
        !showIcon ? "block overflow-hidden text-ellipsis whitespace-nowrap" : "",
        className,
      )}
      {...rest}
    >
      {children}
      {showIcon &&
        (!external ? (
          <SvgLinkInternal className="fill-[#5E5E60] group-hover:fill-white" />
        ) : (
          <SvgLinkExternal className="fill-[#5E5E60] group-hover:fill-white" />
        ))}
    </RouterLink>
  )
})
