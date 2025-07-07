import { useTranslation } from "react-i18next"
import { twMerge } from "tailwind-merge"

import { SvgChevronDownCircle, SvgChevronUpCircle } from "assets"
import { AuthorExtended, AuthorPublications } from "types"

import { PublicationsTable } from "ui/components/specific"

const TEST_AUTHOR_IMG =
  "data:image/png;base64,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"

type PublicationsCollapseBaseProps = {
  expanded: boolean
  items: AuthorPublications[]
  onExpand: (id: string) => void
  onPublicationStoresClick: (id: string) => void
}

export type PublicationsCollapseProps = Pick<AuthorExtended, "id" | "title" | "nickname"> &
  PublicationsCollapseBaseProps

export const PublicationsCollapse = ({
  id,
  nickname,
  title,
  expanded,
  items,
  onExpand,
  onPublicationStoresClick,
}: PublicationsCollapseProps) => {
  const { t } = useTranslation("profile")

  const hasItems = items && items.length > 0

  return (
    <div className={twMerge("flex flex-col rounded-lg border border-gray-300 bg-gray-100")}>
      <div
        className={twMerge("flex items-center gap-4 p-4", hasItems && "cursor-pointer")}
        onClick={hasItems ? () => onExpand(id) : undefined}
      >
        <div className="flex flex-grow gap-3">
          <div className="h-13 w-13 overflow-hidden rounded-full">
            <img src={TEST_AUTHOR_IMG} className="h-full w-full object-cover" />
          </div>
          <div className="flex flex-col justify-center gap-1">
            <span className="text-2sm font-semibold leading-4.5">{nickname}</span>
            <span className="text-2xs leading-4">{title}</span>
          </div>
        </div>
        <span className="w-28 overflow-hidden text-ellipsis whitespace-nowrap">
          {items.length} {t("publication", { count: items.length })}
        </span>
        {hasItems &&
          (expanded ? (
            <SvgChevronDownCircle className="stroke-gray-800" />
          ) : (
            <SvgChevronUpCircle className="stroke-gray-800" />
          ))}
      </div>
      {expanded && <PublicationsTable items={items} onPublicationStoresClick={onPublicationStoresClick} />}
    </div>
  )
}
