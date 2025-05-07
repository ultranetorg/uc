import { memo } from "react"
import { Link } from "react-router-dom"
import { twMerge } from "tailwind-merge"

import { FairSvg, Grid3x3GapFillSvg } from "assets"
import { PropsWithClassName } from "types"
import { FavoritesList, PrimaryButton } from "ui/components"

const FAVORITES = [
  { id: "1", title: "GameNest" },
  { id: "2", title: "SoftVault" },
  { id: "3", title: "MovieMesh" },
]

export const Sidebar = memo(({ className }: PropsWithClassName) => {
  return (
    <div className={twMerge("flex flex-col gap-6 py-8", className)}>
      <FairSvg className="fill-stone-800" />
      <Link to="/">
        <PrimaryButton
          className="w-full"
          image={<Grid3x3GapFillSvg className="fill-stone-800 stroke-stone-800" />}
          label="All Sites"
        />
      </Link>
      <FavoritesList favorites={FAVORITES} isPending={false} />
    </div>
  )
})
