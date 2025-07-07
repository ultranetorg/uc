import { memo, SVGProps } from "react"

export const SvgTwitchColored = memo((props: SVGProps<SVGSVGElement>) => (
  <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg" {...props}>
    <path
      d="M6.00056 0L1.71484 4.28571V19.7143H6.8577V24L11.1434 19.7143H14.572L22.2863 12V0H6.00056ZM20.572 11.1429L17.1434 14.5714H13.7148L10.7148 17.5714V14.5714H6.8577V1.71429H20.572V11.1429Z"
      fill="#2A2932"
    />
    <path d="M18.0014 4.71436H16.2871V9.85721H18.0014V4.71436Z" fill="#2A2932" />
    <path d="M13.2866 4.71436H11.5723V9.85721H13.2866V4.71436Z" fill="#2A2932" />
  </svg>
))
