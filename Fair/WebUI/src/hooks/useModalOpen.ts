import { useState, useCallback } from "react"

export type UseModalOpenReturn = {
  isOpen: boolean
  open: () => void
  close: () => void
  toggle: () => void
}

export const useModalOpen = (initialState = false): UseModalOpenReturn => {
  const [isOpen, setIsOpen] = useState(initialState)

  const open = useCallback(() => setIsOpen(true), [])
  const close = useCallback(() => setIsOpen(false), [])
  const toggle = useCallback(() => setIsOpen(prev => !prev), [])

  return {
    isOpen,
    open,
    close,
    toggle,
  }
}
