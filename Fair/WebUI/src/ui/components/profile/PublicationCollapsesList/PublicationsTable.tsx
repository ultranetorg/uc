import { useTranslation } from "react-i18next"

import { AuthorPublications } from "types"

import { PublicationTableRow } from "./PublicationTableRow"

export type PublicationsTableProps = {
  items: AuthorPublications[]
  onPublicationStoresClick: (id: string) => void
}

export const PublicationsTable = ({ items, onPublicationStoresClick }: PublicationsTableProps) => {
  const { t } = useTranslation("profile")

  return (
    <>
      <div className="flex justify-between bg-gray-200 px-4 py-2 text-2xs font-medium leading-4">
        <span className="w-[40%]">{t("title")}</span>
        <span className="w-[30%]">{t("publicationType")}</span>
        <span className="w-[20%]">{t("publishedIn")}</span>
      </div>
      <div className="divide-y divide-gray-300">
        {items.map(x => (
          <PublicationTableRow key={x.publicationId} {...x} onPublicationStoresClick={onPublicationStoresClick} />
        ))}
      </div>
    </>
  )
}
