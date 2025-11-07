import { memo, SVGProps } from "react"

export const SvgArrowDownSm = memo((props: SVGProps<SVGSVGElement>) => (
  <svg width="10" height="9" viewBox="0 0 10 9" fill="none" xmlns="http://www.w3.org/2000/svg" {...props}>
    <path d="M5 0.5V8.5M5 8.5L9 4.5M5 8.5L1 4.5" strokeLinecap="round" strokeLinejoin="round" />
  </svg>
))
