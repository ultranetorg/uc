import { useState, useEffect, useCallback } from "react"

type UseHeadsObserverProps = {
  offsetTop?: number
}

export const useHeadsObserver = (props: UseHeadsObserverProps = {}) => {
  const { offsetTop: offset = 0 } = props

  const [activeId, setActiveId] = useState<string>()

  const handleScroll = useCallback(() => {
    const top = window.scrollY
    const elements = document.querySelectorAll("h3")

    for (const { offsetTop, id } of elements) {
      if (offsetTop - offset > top) {
        setActiveId(id)
        break
      }
    }
  }, [offset])

  useEffect(() => {
    handleScroll()

    window.addEventListener("scroll", handleScroll)
    return () => window.removeEventListener("scroll", handleScroll)
  }, [handleScroll])

  return activeId
}
