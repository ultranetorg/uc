import { memo, SVGProps } from "react"

export const SvgSliders = memo((props: SVGProps<SVGSVGElement>) => (
  <svg width="16" height="16" viewBox="0 0 16 16" fill="none" xmlns="http://www.w3.org/2000/svg" {...props}>
    <path d="M6.74479 11.9908H1.92204" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" />
    <path
      fillRule="evenodd"
      clipRule="evenodd"
      d="M14.0786 11.9913C14.0786 13.0517 13.219 13.9113 12.1586 13.9113C11.0982 13.9113 10.2386 13.0517 10.2386 11.9913C10.2386 10.9301 11.0982 10.0713 12.1586 10.0713C13.219 10.0713 14.0786 10.9301 14.0786 11.9913Z"
      strokeWidth="1.5"
      strokeLinecap="round"
      strokeLinejoin="round"
    />
    <path d="M9.25514 4.17438H14.0787" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" />
    <path
      fillRule="evenodd"
      clipRule="evenodd"
      d="M1.92173 4.17488C1.92173 5.23606 2.78134 6.09488 3.84173 6.09488C4.90212 6.09488 5.76173 5.23606 5.76173 4.17488C5.76173 3.11449 4.90212 2.25488 3.84173 2.25488C2.78134 2.25488 1.92173 3.11449 1.92173 4.17488Z"
      strokeWidth="1.5"
      strokeLinecap="round"
      strokeLinejoin="round"
    />
  </svg>
))
