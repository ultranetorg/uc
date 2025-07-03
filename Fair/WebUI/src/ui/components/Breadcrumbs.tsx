import React, { memo } from "react"
import { Link } from "react-router-dom"

import { SvgChevronRight, ThreeDotsSvg } from "assets"
import { formatTitle } from "utils"

export type BreadcrumbsItemProps = {
  title: string
  path?: string
}

const BreadcrumbsItem = memo(({ title, path }: BreadcrumbsItemProps) => {
  const formattedTitle = formatTitle(title)

  return path ? (
    <Link className="text-dark-100 hover:font-medium" to={path} title={title}>
      {formattedTitle}
    </Link>
  ) : (
    <span className="text-gray-400" title={title}>
      {formattedTitle}
    </span>
  )
})

export type BreadcrumbsProps = {
  items: BreadcrumbsItemProps[]
}

export const Breadcrumbs = memo(({ items }: BreadcrumbsProps) => {
  if (!items.length) {
    return null
  }

  return (
    <div className="flex h-6 select-none items-center gap-1 text-2xs leading-5">
      {items.length === 1 ? (
        <BreadcrumbsItem {...items[0]} />
      ) : items.length === 2 ? (
        <>
          <BreadcrumbsItem {...items[0]} />
          <SvgChevronRight className="stroke-gray-400" />
          <BreadcrumbsItem {...items[1]} />
        </>
      ) : (
        items.map((x, i) =>
          i === 0 ? (
            <BreadcrumbsItem key={i} {...x} />
          ) : i === 1 ? (
            <React.Fragment key={i}>
              <SvgChevronRight className="stroke-gray-400" />
              <ThreeDotsSvg className="fill-gray-400" />
              <SvgChevronRight className="stroke-gray-400" />
              <BreadcrumbsItem {...x} />
            </React.Fragment>
          ) : (
            <React.Fragment key={i}>
              <SvgChevronRight className="stroke-gray-400" />
              <BreadcrumbsItem {...x} />
            </React.Fragment>
          ),
        )
      )}
    </div>
  )
})
