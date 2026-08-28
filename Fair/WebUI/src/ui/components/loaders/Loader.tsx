import { memo, ReactElement, SVGAttributes, useId } from "react"

export interface IContentLoaderProps extends SVGAttributes<SVGElement> {
  animate?: boolean
  backgroundColor?: string
  backgroundOpacity?: number
  baseUrl?: string
  foregroundColor?: string
  foregroundOpacity?: number
  gradientRatio?: number
  rtl?: boolean
  speed?: number
  title?: string
  uniqueKey?: string
  beforeMask?: ReactElement
}

export const Loader = memo(
  ({
    animate = true,
    backgroundColor = "#e8e9f1",
    backgroundOpacity = 1,
    baseUrl = "",
    children,
    foregroundColor = "#d2d4e4",
    foregroundOpacity = 1,
    gradientRatio = 2,
    uniqueKey,
    rtl = false,
    speed = 1.2,
    style = {},
    title,
    beforeMask,
    ...props
  }: IContentLoaderProps) => {
    const generatedId = useId()
    const fixedId = uniqueKey ?? generatedId
    const idClip = `${fixedId}-diff`
    const idGradient = `${fixedId}-animated-diff`
    const idAria = `${fixedId}-aria`

    const rtlStyle = rtl ? { transform: "scaleX(-1)" } : null
    const dur = `${speed}s`

    const from = `${gradientRatio * -1} 0`
    const to = `${gradientRatio} 0`

    return (
      <svg aria-labelledby={idAria} role="img" style={{ ...style, ...rtlStyle }} {...props}>
        {title && <title id={idAria}>{title}</title>}
        {beforeMask}
        <rect
          role="presentation"
          x="0"
          y="0"
          width="100%"
          height="100%"
          clipPath={`url(${baseUrl}#${idClip})`}
          style={{ fill: `url(${baseUrl}#${idGradient})` }}
        />

        <defs>
          <clipPath id={idClip}>{children}</clipPath>

          <linearGradient id={idGradient} gradientTransform={`translate(${from})`}>
            <stop offset="0%" stopColor={backgroundColor} stopOpacity={backgroundOpacity} />

            <stop offset="50%" stopColor={foregroundColor} stopOpacity={foregroundOpacity} />

            <stop offset="100%" stopColor={backgroundColor} stopOpacity={backgroundOpacity} />

            {animate && (
              <animateTransform
                attributeName="gradientTransform"
                type="translate"
                values={`${from}; 0 0; ${to}`}
                dur={dur}
                repeatCount="indefinite"
              />
            )}
          </linearGradient>
        </defs>
      </svg>
    )
  },
)
