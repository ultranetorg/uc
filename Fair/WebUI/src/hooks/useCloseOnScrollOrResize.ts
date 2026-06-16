import { useEffect } from "react"

export const useCloseOnScrollOrResize = (onCollapse: () => void) => {
  useEffect(() => {
    const handler = () => onCollapse()
    window.addEventListener("resize", handler)
    window.addEventListener("scroll", handler, true)

    return () => {
      window.removeEventListener("resize", handler)
      window.removeEventListener("scroll", handler, true)
    }
  }, [onCollapse])
}
