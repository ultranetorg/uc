import { memo, SVGProps } from "react"

export const ChevronRightSvg = memo((props: SVGProps<SVGSVGElement>) => (
  <svg width="7" height="8" viewBox="0 0 7 8" fill="none" xmlns="http://www.w3.org/2000/svg" {...props}>
    <path
      d="M6.66797 5.0316L0.167969 7.89382V6.02415L4.61825 4.23757L4.55824 4.33452V4.10369L4.61825 4.20064L0.167969 2.41406V0.544389L6.66797 3.4066V5.0316Z"
      fill="#181818"
    />
  </svg>
))
