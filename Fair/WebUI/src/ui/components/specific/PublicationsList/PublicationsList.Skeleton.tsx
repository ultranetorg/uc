import { memo, useMemo } from "react"

import { ListSkeleton, ListSkeletonProps } from "ui/components"

const NAME_CLASSNAME = "max-w-130"
const TYPE_COLUMN = "hidden w-24 shrink-0 md:block"
const TEXT_COLUMN = "hidden max-w-60 flex-1 md:block"
const RATING_COLUMN = "w-12 shrink-0 sm:w-16"
const HIDDEN_RATING_COLUMN = `invisible ${RATING_COLUMN}`

type PublicationsListSkeletonBaseProps = {
  showPublicationType?: boolean
  showRating?: boolean
}

export type PublicationsListSkeletonProps = Pick<ListSkeletonProps, "rows"> & PublicationsListSkeletonBaseProps

export const PublicationsListSkeleton = memo(
  ({ rows, showPublicationType = false, showRating = false }: PublicationsListSkeletonProps) => {
    const columns = useMemo(
      () => [
        ...(showPublicationType ? [TYPE_COLUMN] : []),
        TEXT_COLUMN,
        TEXT_COLUMN,
        showRating ? RATING_COLUMN : HIDDEN_RATING_COLUMN,
      ],
      [showPublicationType, showRating],
    )

    return <ListSkeleton rows={rows} nameClassName={NAME_CLASSNAME} columns={columns} showNameSubtitle />
  },
)
