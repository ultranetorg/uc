import { useEffect, useMemo } from "react"

import { useModerationContext } from "app"
import { Dropdown, DropdownProps } from "ui/components"

import { categoriesToDropdownItems, keepSpacesFormatOptionLabel } from "./utils"

export type DropdownSearchAccountProps = Pick<
  DropdownProps<false>,
  "className" | "controlled" | "error" | "placeholder" | "onChange" | "value"
>

export const DropdownSearchAccount = ({ placeholder, ...rest }: DropdownSearchAccountProps) => {
  const { refetchCategories, categories } = useModerationContext()

  //const { data: currentMembers, isFetching } = useGetSiteMembers(memberType, siteId)
  //const { data: currentMembers } = useGetSiteMembers(memberType, siteId)
  //const { data: searchAccounts } = useSearchAccounts(debouncedSearch)
  const items = useMemo(() => categoriesToDropdownItems(categories), [categories])

  useEffect(() => {
    if (!categories) {
      refetchCategories?.()
    }
  }, [categories, refetchCategories])

  return (
    <Dropdown
      controlled={true}
      isMulti={false}
      isSearchable={true}
      items={items}
      placeholder={placeholder}
      size={"large"}
      formatOptionLabel={keepSpacesFormatOptionLabel}
      {...rest}
    />
  )
}
