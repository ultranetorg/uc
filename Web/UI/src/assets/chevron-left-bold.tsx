import { memo, SVGProps } from "react"

export const SvgChevronLeftBold = memo((props: SVGProps<SVGSVGElement>) => (
  <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg" {...props}>
    <path d="M14 16L10 12L14 8" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
  </svg>
))
