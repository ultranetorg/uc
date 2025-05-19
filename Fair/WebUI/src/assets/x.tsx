import { memo, SVGProps } from "react"

export const XSvg = memo((props: SVGProps<SVGSVGElement>) => (
  <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg" {...props}>
    <path d="M16 16L8 8M16 8L8 16" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" />
  </svg>
))
