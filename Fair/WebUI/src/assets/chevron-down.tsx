import { forwardRef, memo, SVGProps } from "react"

export const ChevronDownSvg = memo(
  forwardRef<SVGSVGElement, SVGProps<SVGSVGElement>>((props: SVGProps<SVGSVGElement>, ref) => (
    <svg ref={ref} width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg" {...props}>
      <path d="M15 11L12 14L9 11" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" />
    </svg>
  )),
)
