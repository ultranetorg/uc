import { useMediaQuery as useMediaQueryHook } from "usehooks-ts"

import { Breakpoints } from "constants"

export const useMediaQuery = (breakpoint: Breakpoints | string): boolean => {
  const parameter = typeof breakpoint === "string" ? breakpoint : `(max-width: ${breakpoint}px)`
  return useMediaQueryHook(parameter)
}
