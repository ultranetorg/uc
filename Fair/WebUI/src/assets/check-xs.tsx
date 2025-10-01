import { memo, SVGProps } from "react"

// stroke="#2A2932"
export const SvgCheckXs = memo((props: SVGProps<SVGSVGElement>) => (
  <svg width="16" height="16" viewBox="0 0 16 16" fill="none" xmlns="http://www.w3.org/2000/svg" {...props}>
    <path
      d="M2.66699 8L5.96682 11.2998L13.0382 4.22876"
      strokeWidth="1.5"
      strokeLinecap="round"
      strokeLinejoin="round"
    />
  </svg>
))
