import { ReactNode, memo, useMemo } from "react"
import Select, { CSSObjectWithLabel, GroupBase, OptionProps, Props } from "react-select"
import AsyncSelect, { AsyncProps } from "react-select/async"

export type SelectOption = {
  value?: string | number
  label: string | number
  icon?: ReactNode
  tag?: any
}

type StylizedSelectBaseProps = {
  options?: SelectOption[]
  value?: string | number
  defaultValue?: string | number
  controlStyle?: object
  dropdownIndicatorStyle?: object
  indicatorSeparatorStyle?: object
  indicatorsContainerStyle?: object
  inputStyle?: object
  menuStyle?: object
  menuListStyle?: object
  optionStyle?: object
  placeholderStyle?: object
  singleValueStyle?: object
  valueContainerStyle?: object
}

type StylizedSelectProps =
  | ({ isSearchable: false } & StylizedSelectBaseProps &
      Omit<Props<SelectOption, false, GroupBase<SelectOption>>, "defaultValue" | "options" | "value">)
  | ({ isSearchable: true } & StylizedSelectBaseProps &
      Omit<AsyncProps<SelectOption, false, GroupBase<SelectOption>>, "defaultValue" | "options" | "value">)

export const StylizedSelect = memo((props: StylizedSelectProps) => {
  const { isSearchable, options, value, defaultValue, ...rest } = props

  const styles = useMemo(
    () => ({
      control: (base: CSSObjectWithLabel) => ({
        ...base,
        borderRadius: "6px",
        border: "1px solid #3DC1F2",
        boxShadow: "none",
        color: "#3DC1F2",
        cursor: "pointer",
        backgroundColor: "#132C38",
        "&:hover": {
          borderColor: "#3DC1F2",
        },
        ...props.controlStyle,
      }),
      dropdownIndicator: (base: CSSObjectWithLabel) => ({
        ...base,
        color: "#FFF",
        "&:hover": {
          color: "#FFF",
        },
        ...props.dropdownIndicatorStyle,
      }),
      indicatorSeparator: (base: CSSObjectWithLabel) => ({
        ...base,
        display: "none",
        ...props.indicatorSeparatorStyle,
      }),
      indicatorsContainer: (base: CSSObjectWithLabel) => ({
        ...base,
        ...props.indicatorsContainerStyle,
      }),
      // Search input
      input: (base: CSSObjectWithLabel) => ({
        ...base,
        ":hover": {
          cursor: "text",
        },
        ...props.inputStyle,
      }),
      menu: (base: CSSObjectWithLabel) => ({
        ...base,
        border: "1px solid #3DC1F2",
        backgroundColor: "#132C38",
        color: "#FFF",
        fontSize: "14px",
        marginTop: "0px",
        ...props.menuStyle,
      }),
      menuList: (base: CSSObjectWithLabel) => ({
        ...base,
        ...props.menuListStyle,
      }),
      option: (baseStyle: CSSObjectWithLabel, optionProps: OptionProps<SelectOption>) => ({
        ...baseStyle,
        backgroundColor: optionProps.isSelected || optionProps.isFocused ? "#376782" : "#132C38",
        cursor: "pointer",
        "&:hover": {
          backgroundColor: "#3DC1F2",
        },
        ...props.optionStyle,
      }),
      // Placeholder
      placeholder: (base: CSSObjectWithLabel) => ({
        ...base,
        overflow: "hidden",
        textOverflow: "ellipsis",
        whiteSpace: "nowrap",
        ...props.placeholderStyle,
      }),
      // Selected item in Select
      singleValue: (base: CSSObjectWithLabel) => ({
        ...base,
        color: "#FFF",
        fontSize: "14px",
        ...props.singleValueStyle,
      }),

      valueContainer: (base: CSSObjectWithLabel) => ({
        ...base,
        ...props.valueContainerStyle,
      }),
    }),
    [
      props.controlStyle,
      props.dropdownIndicatorStyle,
      props.indicatorSeparatorStyle,
      props.indicatorsContainerStyle,
      props.inputStyle,
      props.menuStyle,
      props.menuListStyle,
      props.optionStyle,
      props.placeholderStyle,
      props.singleValueStyle,
      props.valueContainerStyle,
    ],
  )

  const currentDefaultValue = useMemo(
    () => (defaultValue !== undefined ? options?.find(x => x.value === defaultValue) : undefined),
    [options, defaultValue],
  )
  const currentValue = useMemo(() => options?.find(x => x.value === value) ?? [], [options, value])

  return !isSearchable ? (
    <Select
      defaultValue={currentDefaultValue}
      value={currentValue}
      // @ts-ignore
      styles={styles}
      isSearchable={false}
      isMulti={false}
      menuPortalTarget={document.body}
      options={options}
      {...rest}
    />
  ) : (
    <AsyncSelect
      defaultValue={currentDefaultValue}
      value={currentValue}
      // @ts-ignore
      styles={styles}
      isSearchable={true}
      isMulti={false}
      menuPortalTarget={document.body}
      {...rest}
    />
  )
})
