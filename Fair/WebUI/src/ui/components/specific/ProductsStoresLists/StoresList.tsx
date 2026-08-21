import { memo } from "react"
import { useTranslation } from "react-i18next"
import { Link } from "react-router-dom"

import { StoreBase } from "types"
import { List, ListHeader } from "ui/components"
import { routes } from "utils"

import { StoreRow } from "./StoreRow"
import { COLUMN_NAME_CLASSNAME } from "./styles"

export type StoresListProps = {
  items: StoreBase[]
}

export const StoresList = memo(({ items }: StoresListProps) => {
  const { t } = useTranslation("common")

  return (
    <List
      header={
        <ListHeader className="capitalize leading-4.25">
          <span className={COLUMN_NAME_CLASSNAME}>{t("store")}</span>
        </ListHeader>
      }
      className="text-2xs leading-4"
    >
      {items.map(x => (
        <Link className="block" to={routes.store(x.id)} key={x.id}>
          <StoreRow title={x.title} imageFileId={x.imageFileId} />
        </Link>
      ))}
    </List>
  )
})
