import { memo, SVGProps } from "react"

// stroke="#D51C28"
export const SvgXCircleSm = memo((props: SVGProps<SVGSVGElement>) => (
  <svg width="20" height="20" viewBox="0 0 20 20" fill="none" xmlns="http://www.w3.org/2000/svg" {...props}>
    <path
      d="M9.99984 18.3334C14.5832 18.3334 18.3332 14.5834 18.3332 10.0001C18.3332 5.41675 14.5832 1.66675 9.99984 1.66675C5.4165 1.66675 1.6665 5.41675 1.6665 10.0001C1.6665 14.5834 5.4165 18.3334 9.99984 18.3334Z"
      strokeWidth="1.5"
      strokeLinecap="round"
      strokeLinejoin="round"
    />
    <path d="M7.6416 12.3583L12.3583 7.6416" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" />
    <path d="M12.3583 12.3583L7.6416 7.6416" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" />
  </svg>
))
