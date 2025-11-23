import { useEffect } from "react"

export function useScrollOrResize(callback: () => void, enabled = true) {
  useEffect(() => {
    if (!enabled) return

    const handler = () => callback()

    window.addEventListener("resize", handler)
    window.addEventListener("scroll", handler, true)

    return () => {
      window.removeEventListener("resize", handler)
      window.removeEventListener("scroll", handler, true)
    }
  }, [callback, enabled])
}
