import { memo, useCallback, useEffect, useState } from "react"
import { twMerge } from "tailwind-merge"

import { useHeadsObserver } from "hooks"
import { PropsWithClassName } from "types"

type Heading = {
  id: string
  text: string
  level: number
}

type TableOfContentBaseProps = {
  title?: string | null
}

type TableOfContentProps = PropsWithClassName<TableOfContentBaseProps>

export const TableOfContent = memo((props: TableOfContentProps) => {
  const { className, title } = props

  const [headings, setHeadings] = useState<Heading[]>([])

  const activeId = useHeadsObserver({ offsetTop: 56 })

  const handleClick = useCallback((e: any, heading: Heading) => {
    e.preventDefault()

    const h3 = document.querySelector(`#${heading.id}`)
    if (h3) {
      const y = h3.getBoundingClientRect().top + window.scrollY - 57
      window.scrollTo({
        top: y,
        behavior: "smooth",
      })
    }
  }, [])

  const getClassName = (level: number) => {
    switch (level) {
      case 2:
        return "head2"
      case 3:
        return "head3"
      case 4:
        return "head4"
      default:
        return undefined
    }
  }

  useEffect(() => {
    const elements = Array.from(document.querySelectorAll("h3"))
    const mapped = elements.map(elem => ({
      id: elem.id,
      text: elem.innerHTML,
      level: +elem.nodeName.charAt(1),
    }))
    setHeadings(mapped)
  }, [])

  return (
    <div className={twMerge("flex flex-col rounded-md bg-[#1A1A1D]", className)}>
      {title && <div className="flex select-none items-center px-4 py-2 text-sm uppercase leading-[26px]">{title}</div>}
      <ul>
        {headings.map(heading => (
          <li
            key={heading.id}
            onClick={e => handleClick(e, heading)}
            className={twMerge(
              getClassName(heading.level),
              "group flex cursor-pointer items-center rounded-md px-4 py-2 text-sm uppercase leading-[26px] text-white",
              heading.id === activeId ? "bg-[#132C38]" : "",
            )}
          >
            <a
              href={`#${heading.id}`}
              className={twMerge("group-hover:underline", heading.id === activeId ? "text-white" : "")}
            >
              {heading.text}
            </a>
          </li>
        ))}
      </ul>
    </div>
  )
})
