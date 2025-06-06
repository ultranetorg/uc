import { memo, useEffect, useRef, useState } from "react"
import { Link } from "react-router-dom"
import { twMerge } from "tailwind-merge"

import { CategoryBase } from "types"

import { CategoryCard } from "./CategoryCard"
import { MoreDropdownButton } from "./MoreDropdownButton"
import { getVisibleItemsCount } from "./utils"

export type CategoriesListProps = {
  siteId: string
  categories: CategoryBase[]
}

export const CategoriesList = memo(({ siteId, categories }: CategoriesListProps) => {
  const containerRef = useRef<HTMLDivElement>(null)
  const itemRefs = useRef<(HTMLAnchorElement | null)[]>([])
  const [visibleCount, setVisibleCount] = useState(categories.length)

  useEffect(() => {
    const handleResize = () => {
      const count = getVisibleItemsCount(containerRef, itemRefs)
      setVisibleCount(count)
    }

    handleResize()

    window.addEventListener("resize", handleResize)
    return () => {
      window.removeEventListener("resize", handleResize)
    }
  }, [])

  const hiddenCategories = categories.slice(visibleCount)

  return (
    <div className="flex w-full">
      <div className="relative flex flex-grow gap-3 overflow-hidden" ref={containerRef}>
        <>
          {categories.map((x, i) => (
            <Link
              key={x.id}
              to={`/${siteId}/c/${x.id}`}
              ref={el => (itemRefs.current[i] = el)}
              className={twMerge(i >= visibleCount && "invisible")}
            >
              <CategoryCard key={x.id} title={x.title} />
            </Link>
          ))}

          <div className="pointer-events-none absolute right-0 top-0 h-full w-16 bg-gradient-to-l from-white to-transparent"></div>
        </>
      </div>
      {hiddenCategories.length > 0 && <MoreDropdownButton siteId={siteId} items={hiddenCategories} />}
    </div>
  )
})
