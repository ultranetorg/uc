import { memo, useCallback, useEffect, useState } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

import { useHeadsObserver } from "./hooks"

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
      const layout = document.getElementById("layout")!
      const y = h3.getBoundingClientRect().top + layout.scrollTop - 57
      layout.scrollTo({
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
    <div
      className={twMerge(
        "flex select-none flex-col gap-4 rounded-lg border border-dark-alpha-100 bg-dark-alpha-100 px-4 py-6 text-sm uppercase leading-[17px] text-gray-200 backdrop-blur-[5px]",
        className,
      )}
    >
      {title && <div className="mx-4 font-bold">{title}</div>}
      <ul>
        {headings.map(heading => (
          <li
            key={heading.id}
            onClick={e => handleClick(e, heading)}
            className={twMerge(
              getClassName(heading.level),
              "group flex cursor-pointer items-center rounded px-4 py-3",
              heading.id === activeId ? "bg-cyan-950" : "",
            )}
          >
            <a href={`#${heading.id}`} className="group-hover:underline">
              {heading.text}
            </a>
          </li>
        ))}
      </ul>
    </div>
  )
})
