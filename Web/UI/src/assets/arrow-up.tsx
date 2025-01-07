import { memo, SVGProps } from "react"

export const SvgArrowUp = memo((props: SVGProps<SVGSVGElement>) => (
  <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg" {...props}>
    <path d="M12 19L12 5M12 5L6 11M12 5L18 11" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
  </svg>
))
