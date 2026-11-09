import { memo, useMemo } from "react"

import { ListSkeleton, ListSkeletonProps } from "ui/components"

const NAME_CLASSNAME = "max-w-130"
const TYPE_COLUMN = "w-24 shrink-0"
const TEXT_COLUMN = "max-w-60 flex-1"
const RATING_COLUMN = "w-16 shrink-0"
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

    return <ListSkeleton rows={rows} nameClassName={NAME_CLASSNAME} columns={columns} />
  },
)
