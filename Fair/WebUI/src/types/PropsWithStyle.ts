import { CSSProperties } from "react"

export type PropsWithStyle<P = unknown> = P & { style?: CSSProperties }
