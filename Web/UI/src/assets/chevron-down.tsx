import { memo, SVGProps } from "react"

export const SvgChevronDown = memo((props: SVGProps<SVGSVGElement>) => (
  <svg width="16" height="16" viewBox="0 0 16 16" fill="none" xmlns="http://www.w3.org/2000/svg" {...props}>
    <path d="M3 6.5L7.5 11L12 6.5" strokeLinecap="round" strokeLinejoin="round" />
  </svg>
))
