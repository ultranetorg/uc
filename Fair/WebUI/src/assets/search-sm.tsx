import { memo, SVGProps } from "react"

// stroke="#737582"
export const SvgSearchSm = memo((props: SVGProps<SVGSVGElement>) => (
  <svg width="20" height="20" viewBox="0 0 20 20" fill="none" xmlns="http://www.w3.org/2000/svg" {...props}>
    <path
      d="M9.58464 17.4998C13.9569 17.4998 17.5013 13.9554 17.5013 9.58317C17.5013 5.21092 13.9569 1.6665 9.58464 1.6665C5.21238 1.6665 1.66797 5.21092 1.66797 9.58317C1.66797 13.9554 5.21238 17.4998 9.58464 17.4998Z"
      strokeWidth="1.5"
      strokeLinecap="round"
      strokeLinejoin="round"
    />
    <path d="M18.3346 18.3332L16.668 16.6665" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" />
  </svg>
))
