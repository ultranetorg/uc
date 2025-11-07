import { useState, useEffect } from "react"

type UseHeadsObserverProps = {
  offsetTop?: number
}

export const useHeadsObserver = (props: UseHeadsObserverProps = {}) => {
  const { offsetTop: offset = 0 } = props

  const [activeId, setActiveId] = useState<string>()

  useEffect(() => {
    const layout = document.getElementById("layout")!

    const handleScroll = () => {
      const top = layout.scrollTop
      const elements = document.querySelectorAll("h3")

      for (const { offsetTop, id } of elements) {
        if (offsetTop - offset > top) {
          setActiveId(id)
          break
        }
      }
    }

    handleScroll()
    layout.addEventListener("scroll", handleScroll)
    return () => layout.removeEventListener("scroll", handleScroll)
  }, [offset])

  return activeId
}
