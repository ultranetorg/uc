import { memo, SVGProps } from "react"

export const SvgChevronLeft = memo((props: SVGProps<SVGSVGElement>) => (
  <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg" {...props}>
    <path d="M13 15L10 12L13 9" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" />
  </svg>
))
