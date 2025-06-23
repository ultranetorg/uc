import { memo, SVGProps } from "react"

export const SvgXSm = memo((props: SVGProps<SVGSVGElement>) => (
  <svg width="20" height="20" viewBox="0 0 20 20" fill="none" xmlns="http://www.w3.org/2000/svg" {...props}>
    <path
      d="M13.3333 13.3336L6.66667 6.66699M13.3334 6.66699L6.66667 13.3337"
      strokeWidth="1.5"
      strokeLinecap="round"
      strokeLinejoin="round"
    />
  </svg>
))
