import { KeyboardEvent, ReactNode, useId, useState } from "react"
import { SvgChevronRightMd } from "../../assets"

export interface AccordionItem {
  id?: string
  title: string
  content: ReactNode
}

export interface AccordionProps {
  items: AccordionItem[]
  multi?: true
  defaultOpenIds?: string[]
  className?: string
}

export const Accordion = ({ items, multi, defaultOpenIds = [], className = "" }: AccordionProps) => {
  const baseId = useId()
  const [openIds, setOpenIds] = useState<string[]>(() => {
    if (multi) return defaultOpenIds.slice()
    return defaultOpenIds.length ? [defaultOpenIds[0]] : []
  })

  function toggle(id: string) {
    if (multi) {
      if (openIds.includes(id)) {
        setOpenIds(openIds.filter(x => x !== id))
        return
      }
      setOpenIds([...openIds, id])
      return
    }

    setOpenIds(openIds.includes(id) ? [] : [id])
  }

  const onKeyDown = (e: KeyboardEvent, id: string) => {
    if (e.key === "Enter" || e.key === " ") {
      e.preventDefault()
      toggle(id)
    }
  }

  return (
    <div className={`w-full ${className}`}>
      {items.map((it, idx) => {
        const id = it.id ?? `${baseId}-item-${idx}`
        const headerId = `${id}-header`
        const panelId = `${id}-panel`
        const isOpen = openIds.includes(id)

        return (
          <div key={id} className={`px-2 ${isOpen ? "bg-slate-50" : ""}`}>
            <div
              id={headerId}
              role="button"
              tabIndex={0}
              aria-expanded={isOpen}
              aria-controls={panelId}
              onClick={() => toggle(id)}
              onKeyDown={e => onKeyDown(e, id)}
              className={`mb-2 w-full cursor-pointer transition-transform duration-200 ${isOpen ? "" : "hover:underline"}`}
            >
              <div className="flex justify-between">
                <span className="text-2xs leading-6 text-gray-600">{it.title}</span>
                <SvgChevronRightMd
                  className={`fill-gray-400 transition-transform duration-150 hover:fill-gray-500 ${isOpen ? "rotate-90" : "rotate-0"}`}
                />
              </div>
            </div>

            <div
              id={panelId}
              role="region"
              aria-labelledby={headerId}
              className={`transition-[opacity,padding] duration-200 ${isOpen ? "py-2 opacity-100" : "hidden p-0 opacity-0"}`}
            >
              <div className="text-sm text-gray-700">{it.content}</div>
            </div>
          </div>
        )
      })}
    </div>
  )
}

export default Accordion
