import { memo, SVGProps } from "react"

// stroke="#737582" (stroke-gray-500)
export const SvgXXs = memo((props: SVGProps<SVGSVGElement>) => (
  <svg width="16" height="16" viewBox="0 0 16 16" fill="none" xmlns="http://www.w3.org/2000/svg" {...props}>
    <path
      d="M10.6663 10.6666L5.33301 5.33331M10.6664 5.33331L5.33301 10.6667"
      strokeWidth="1.5"
      strokeLinecap="round"
      strokeLinejoin="round"
    />
  </svg>
))
