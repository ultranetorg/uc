import { useState, useEffect } from "react"
import { createPortal } from "react-dom"

import { ScrollToTopButton } from "ui/components"

export const ScrollToTop = () => {
  const [show, setShow] = useState(false)

  useEffect(() => {
    const onScroll = () => {
      setShow(window.scrollY > 300)
    }
    window.addEventListener("scroll", onScroll)
    return () => window.removeEventListener("scroll", onScroll)
  }, [])

  const scrollToTop = () => {
    window.scrollTo({ top: 0, behavior: "smooth" })
  }

  return !show
    ? null
    : createPortal(
        <ScrollToTopButton
          className="fixed bottom-6 right-6 transition"
          onClick={scrollToTop}
          aria-label="Scroll to top"
        />,
        document.body,
      )
}
