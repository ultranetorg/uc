import { useWindowSize } from "usehooks-ts"

import { Breakpoints } from "constants"

export const useBreakpoint = (): Breakpoints => {
  const { width } = useWindowSize()

  if (width < Breakpoints.xs) {
    return Breakpoints.xs
  } else if (width < Breakpoints.sm) {
    return Breakpoints.sm
  } else if (width < Breakpoints.md) {
    return Breakpoints.md
  } else if (width < Breakpoints.lg) {
    return Breakpoints.lg
  } else if (width < Breakpoints.xl) {
    return Breakpoints.lg
  }

  return Breakpoints.xl2
}
