import { memo, SVGProps } from "react"

export const SvgDropdownIndicator = memo((props: SVGProps<SVGSVGElement>) => (
  <svg width="16" height="16" viewBox="0 0 16 16" fill="none" xmlns="http://www.w3.org/2000/svg" {...props}>
    <path
      d="M12.6666 5.66666L7.99992 10.3333L3.33325 5.66666"
      stroke="#737582"
      stroke-width="1.5"
      stroke-linecap="round"
      stroke-linejoin="round"
    />
  </svg>
))
