import { useEffect, useRef } from "react"

// Стек активных подписчиков Escape: событие обрабатывает только самый верхний
// (последний смонтированный), например открытая поверх fullscreen-страницы модалка.
const stack: symbol[] = []

export function useEscapeKey(onEscape?: () => void) {
  const idRef = useRef(Symbol())

  useEffect(() => {
    if (!onEscape) return undefined

    const id = idRef.current
    stack.push(id)
    return () => {
      const index = stack.indexOf(id)
      if (index !== -1) stack.splice(index, 1)
    }
  }, [onEscape])

  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key !== "Escape") return
      if (stack[stack.length - 1] !== idRef.current) return

      onEscape?.()
    }

    document.addEventListener("keydown", handleKeyDown)
    return () => {
      document.removeEventListener("keydown", handleKeyDown)
    }
  }, [onEscape])
}
