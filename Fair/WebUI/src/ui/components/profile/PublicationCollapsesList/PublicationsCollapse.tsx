import { useTranslation } from "react-i18next"
import { twMerge } from "tailwind-merge"

import { SvgChevronDownCircle, SvgChevronUpCircle } from "assets"
import { TEST_AUTHOR_IMG } from "testConfig"
import { AuthorExtended, AuthorPublications } from "types"
import { PublicationsTable } from "ui/components/specific"

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
      {expanded && <PublicationsTable items={[]} onPublicationStoresClick={onPublicationStoresClick} />}
    </div>
  )
}
