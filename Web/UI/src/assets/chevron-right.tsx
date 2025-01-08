import { memo, SVGProps } from "react"

export const SvgChevronRight = memo((props: SVGProps<SVGSVGElement>) => (
  <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg" {...props}>
    <path d="M11 9L14 12L11 15" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" />
  </svg>
))
