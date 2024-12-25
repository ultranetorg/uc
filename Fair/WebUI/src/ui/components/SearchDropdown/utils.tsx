import { ReactNode } from "react"

import { SvgGlobe, SvgPerson, SvgPersonSquare } from "assets"
import { Search, AccountSearch, AuthorSearch, ResourceSearch, SearchType } from "types"
import { SelectOption } from "ui/components"
import { parseAddress } from "utils"

import { HighlightedText } from "./HighlightedText"

const searchTypeToIconMap = new Map<SearchType, ReactNode>([
  ["accountSearch", <SvgPersonSquare className="h-[18px] w-[18px] fill-white" />],
  ["authorSearch", <SvgPerson className="h-[18px] w-[18px] fill-white" />],
  ["resourceSearch", <SvgGlobe className="h-[18px] w-[18px] fill-white" />],
])

const getOptionValue = (item: Search): string | undefined => {
  switch (item.$type) {
    case "accountSearch":
      return (item as AccountSearch)?.address
    case "authorSearch":
      return (item as AuthorSearch)?.name
    case "resourceSearch":
      return (item as ResourceSearch)?.resource
  }
}

export const getNavigatePath = (type: SearchType, value: string): string | undefined => {
  switch (type) {
    case "accountSearch":
      return `/accounts/${value}`
    case "authorSearch":
      return `/authors/${value}`
    case "resourceSearch": {
      const address = parseAddress(value)
      return address ? `/authors/${address.author}/resources/${address.resource}` : undefined
    }
  }
}

export const mapBaseSearchToSelectOption = (item: Search): SelectOption => {
  const value = getOptionValue(item)

  return {
    icon: searchTypeToIconMap.get(item.$type),
    label: value ?? "",
    value: value?.toLowerCase(),
    tag: value ? getNavigatePath(item.$type, value) : undefined,
  }
}

export const formatOption = ({ icon, label }: any, { inputValue }: any) => {
  return (
    <div className="flex items-center gap-2">
      {icon} <HighlightedText text={label} textToHighlight={inputValue} />
    </div>
  )
}
