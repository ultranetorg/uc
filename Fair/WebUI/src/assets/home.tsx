import * as React from "react"

export const HomeSvg = (props: React.SVGProps<SVGSVGElement>) => (
  <svg
    viewBox="0 0 24 24"
    fill="none"
    stroke="currentColor"
    strokeWidth={1.5}
    strokeLinecap="round"
    strokeLinejoin="round"
    {...props}
  >
    <path d="M3 11.5L12 4l9 7.5" />
    <path d="M4.5 10.5V20a1 1 0 001 1h13a1 1 0 001-1v-9.5" />
    <path d="M9 21V14h6v7" />
  </svg>
)

