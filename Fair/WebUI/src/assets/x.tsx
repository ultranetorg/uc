import { memo, SVGProps } from "react"

export const SvgX = memo((props: SVGProps<SVGSVGElement>) => (
  <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg" {...props}>
    <path d="M18 18L6 6M18 6L6 18" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" />
  </svg>
))
