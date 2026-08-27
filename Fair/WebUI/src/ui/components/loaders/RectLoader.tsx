import { memo } from "react"
import { Loader } from "./Loader"

export interface RectLoaderProps {
  width: number
  height: number
  rx?: number
  className?: string
}

export const RectLoader = memo(({ width, height, rx, className }: RectLoaderProps) => (
  <Loader viewBox={`0 0 ${width} ${height}`} style={{ width, height }} className={className}>
    <rect x="0" y="0" width={width} height={height} rx={rx} />
  </Loader>
))
