import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

import { Loader } from "./loaders"

// `light` is for the page, `dark` is for the dark header.
const TONES = {
  light: { background: "#e8e9f1", foreground: "#d2d4e4" },
  dark: { background: "#525361", foreground: "#737582" },
} as const

const RADIUS = { none: 0, xs: 2, sm: 4, md: 6, lg: 8, xl: 12, "2xl": 16, full: "50%" } as const

export type SkeletonBlockRounded = keyof typeof RADIUS

export type SkeletonBlockTone = keyof typeof TONES

type SkeletonBlockBaseProps = {
  rounded?: SkeletonBlockRounded
  tone?: SkeletonBlockTone
}

export type SkeletonBlockProps = PropsWithClassName & SkeletonBlockBaseProps

export const SkeletonBlock = memo(({ className, rounded = "md", tone = "light" }: SkeletonBlockProps) => (
  <Loader
    aria-hidden="true"
    role="presentation"
    className={twMerge("block min-w-0", className)}
    backgroundColor={TONES[tone].background}
    foregroundColor={TONES[tone].foreground}
  >
    <rect x="0" y="0" width="100%" height="100%" rx={RADIUS[rounded]} />
  </Loader>
))
