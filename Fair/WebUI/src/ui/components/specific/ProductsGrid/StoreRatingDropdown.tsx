import { memo, useCallback, useMemo, useState } from "react"
import { components, OptionProps, SingleValue as SingleValueType, SingleValueProps, StylesConfig } from "react-select"
import { twMerge } from "tailwind-merge"
import { SvgStarXxs } from "assets"

import { DropdownItem, DropdownProps, dropdownStyle } from "ui/components"
import { CustomSelect, DropdownIndicator } from "ui/components/Dropdown/components"
import { formatRating } from "utils"

const getStoreRatingDropdownStyle = (isHover: boolean): StylesConfig<DropdownItem, boolean> => ({
  ...dropdownStyle,
  control: (base, props) => ({
    ...dropdownStyle.control?.(base, props),
    transition: undefined,
    border: "none",
    fontSize: "13px",
    height: "auto",
    minHeight: "auto",
    "&:hover": {
      borderColor: "transparent",
    },
  }),
  dropdownIndicator: base => ({
    ...base,
    padding: "0 0 0 6px",
    opacity: isHover ? 1 : 0,
    pointerEvents: isHover ? "auto" : "none",
  }),
  menu: (base, props) => ({
    ...dropdownStyle.menu?.(base, props),
    marginBottom: "0",
  }),
  menuList: base => ({
    ...dropdownStyle.menuList?.(base, {} as never),
    maxHeight: "400px",
    overflowY: "auto",
  }),
  singleValue: base => ({ ...base, color: "#737582" }),
  valueContainer: base => ({
    ...base,
    padding: "0",
  }),
})

export type StoreRatingDropdownItem = DropdownItem & {
  publicationId: string
  rating: number
}

export const SHOW_ALL_VALUE = "show-all"

const showAllItem: DropdownItem = { value: SHOW_ALL_VALUE, label: "Show all" }

export type StoreRatingDropdownProps = Omit<DropdownProps<false>, "styles" | "defaultValue" | "controlled"> & {
  isFlashing: boolean
  onOpenChange?: (isOpen: boolean) => void
  onShowAllClick?: () => void
}

const Option = (props: OptionProps<DropdownItem, false>) => {
  if (props.data.value === SHOW_ALL_VALUE) {
    return (
      <components.Option {...props}>
        <span className="truncate text-2xs leading-4.5 text-gray-500">{props.data.label}</span>
      </components.Option>
    )
  }

  const { rating, label } = props.data as StoreRatingDropdownItem
  const formattedRating = rating > 0 ? formatRating(rating) : "N/A"

  return (
    <components.Option {...props}>
      <div className="flex items-center text-2xs leading-4.5 text-gray-500 transition-colors duration-700 ease-out">
        <div className="flex min-w-10 max-w-10 items-center">
          {rating > 0 ? (
            <>
              {formattedRating} <SvgStarXxs className="fill-favorite" />
            </>
          ) : (
            "N/A"
          )}
        </div>
        <span className="truncate">{label}</span>
      </div>
    </components.Option>
  )
}

const SingleValue = (props: SingleValueProps<DropdownItem, false>) => {
  const { rating, label } = props.data as StoreRatingDropdownItem
  const formattedRating = rating > 0 ? formatRating(rating) : "N/A"
  return (
    <components.SingleValue {...props}>
      <div className="flex items-center justify-center rounded-sm">
        <div className="flex items-center">
          {rating > 0 ? (
            <>
              {formattedRating} <SvgStarXxs className="fill-favorite" />
            </>
          ) : (
            "N/A"
          )}
        </div>
        &nbsp;at&nbsp;
        <span className="truncate">{label}</span>
      </div>
    </components.SingleValue>
  )
}

const StoreRatingDropdownInner = ({
  className,
  items,
  value,
  isFlashing,
  onChange,
  onOpenChange,
  onShowAllClick,
  ...rest
}: StoreRatingDropdownProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isHover, setHover] = useState(false)

  const storeRatingDropdownStyle = useMemo(() => getStoreRatingDropdownStyle(isHover), [isHover])

  const options = useMemo(() => (onShowAllClick ? [...(items ?? []), showAllItem] : items), [items, onShowAllClick])

  const handleOpenChange = useCallback(
    (open: boolean) => {
      setIsOpen(open)
      onOpenChange?.(open)
    },
    [onOpenChange],
  )

  const currentValue = items?.find(x => x.value === value)

  const handleChange = useCallback(
    (item: SingleValueType<DropdownItem>) => {
      if (item?.value === SHOW_ALL_VALUE) {
        onShowAllClick!()
        return
      }

      const nextItem = item as StoreRatingDropdownItem | null

      if (nextItem) {
        onChange?.(nextItem)
      }

      handleOpenChange(false)
    },
    [onChange, onShowAllClick, handleOpenChange],
  )

  return (
    <div
      className="group"
      data-flashing={isFlashing}
      onClick={e => {
        e.preventDefault()
        e.stopPropagation()
      }}
      onMouseEnter={() => setHover(true)}
      onMouseLeave={() => setHover(false)}
    >
      <CustomSelect<false>
        className={twMerge(className)}
        classNames={{
          control: () =>
            "pl-[20px] bg-gray-100 transition-colors duration-700 ease-out group-data-[flashing=true]:bg-gray-250 group-data-[flashing=true]:duration-0",
        }}
        components={{
          //Control,
          ClearIndicator: () => null,
          IndicatorSeparator: null,
          DropdownIndicator,
          Option,
          SingleValue,
        }}
        closeMenuOnSelect={false}
        hideSelectedOptions={false}
        isSearchable={false}
        options={options}
        styles={storeRatingDropdownStyle}
        value={currentValue ?? null}
        onChange={handleChange}
        menuIsOpen={isOpen}
        menuPlacement="auto"
        onMenuOpen={() => handleOpenChange(true)}
        onMenuClose={() => handleOpenChange(false)}
        {...rest}
      />
    </div>
  )
}

export const StoreRatingDropdown = memo(StoreRatingDropdownInner) as (props: StoreRatingDropdownProps) => JSX.Element
