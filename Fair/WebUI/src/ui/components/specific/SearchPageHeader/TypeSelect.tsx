import { sortBy } from "lodash"
import { memo, useCallback, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"

import { ProductType } from "types"
import { DropdownItem, DropdownSecondary, DropdownSecondaryProps } from "ui/components"

const TYPE_ORDER: ProductType[] = ["software", "game", "movie", "music", "book"]

const sortAvailableTypes = (types: ProductType[]) => sortBy(types, type => TYPE_ORDER.indexOf(type))

type TypeSelectBaseProps = {
  availableTypes: ProductType[] | undefined
  value: ProductType | null
}

export type TypeSelectProps = Pick<DropdownSecondaryProps<false>, "onChange"> & TypeSelectBaseProps

export const TypeSelect = memo(({ availableTypes, value, onChange }: TypeSelectProps) => {
  const { t } = useTranslation()

  const [isOpen, setIsOpen] = useState(false)

  const items = useMemo(
    () =>
      availableTypes && [
        { label: t("common:any"), value: null },
        ...sortAvailableTypes(availableTypes).map<DropdownItem>(x => ({ label: t("categoryTypes:" + x), value: x })),
      ],
    [availableTypes, t],
  )

  const handleMouseEnter = useCallback(() => setIsOpen(true), [])
  const handleMouseLeave = useCallback(() => setIsOpen(false), [])
  const handleMenuOpen = useCallback(() => setIsOpen(true), [])
  const handleMenuClose = useCallback(() => setIsOpen(false), [])

  if (!availableTypes) return null

  return (
    <div onMouseEnter={handleMouseEnter} onMouseLeave={handleMouseLeave}>
      <DropdownSecondary
        isMulti={false}
        items={items}
        className="w-37.5"
        placeholder={t("common:any")}
        defaultValue={null}
        controlled={true}
        value={value ?? undefined}
        menuIsOpen={isOpen}
        onMenuOpen={handleMenuOpen}
        onMenuClose={handleMenuClose}
        onChange={onChange}
      />
    </div>
  )
})
