import React from "react"
import { memo } from "react"
import { Link } from "react-router-dom"

import { ChevronRightSvg, ThreeDotsSvg } from "assets"

export type BreadcrumbsItemProps = {
  title: string
  path?: string
}

const BreadcrumbsItem = memo(({ title, path }: BreadcrumbsItemProps) =>
  path ? (
    <Link className="text-[#0B5CAB]" to={path}>
      {title}
    </Link>
  ) : (
    title
  ),
)

export type BreadcrumbsProps = {
  items: BreadcrumbsItemProps[]
}

export const Breadcrumbs = memo(({ items }: BreadcrumbsProps) => {
  if (!items.length) {
    return null
  }

  return (
    <div className="flex h-5 items-center gap-1 text-[13px] leading-[20px]">
      {items.length === 1 ? (
        <BreadcrumbsItem {...items[0]} />
      ) : items.length === 2 ? (
        <>
          <BreadcrumbsItem {...items[0]} />
          <ChevronRightSvg />
          <BreadcrumbsItem {...items[1]} />
        </>
      ) : (
        items.map((x, i) =>
          i === 0 ? (
            <BreadcrumbsItem key={i} {...x} />
          ) : i === 1 ? (
            <React.Fragment key={i}>
              <ChevronRightSvg />
              <ThreeDotsSvg />
              <ChevronRightSvg />
              <BreadcrumbsItem {...x} />
            </React.Fragment>
          ) : (
            <React.Fragment key={i}>
              <ChevronRightSvg />
              <BreadcrumbsItem {...x} />
            </React.Fragment>
          ),
        )
      )}
    </div>
  )
})
