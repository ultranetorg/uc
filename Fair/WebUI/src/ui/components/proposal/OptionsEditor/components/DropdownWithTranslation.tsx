import { useMemo } from "react"
import { useTranslation } from "react-i18next"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"
import { Dropdown, DropdownProps } from "ui/components"

type DropdownWithTranslationBaseProps = {
  translationKey: string
  items: string[]
}

export type DropdownWithTranslationProps<IsMulti extends boolean> = PropsWithClassName &
  DropdownWithTranslationBaseProps &
  Pick<DropdownProps<IsMulti>, "isMulti" | "error" | "placeholder" | "size" | "onChange">

export const DropdownWithTranslationInner = <IsMulti extends boolean>({
  isMulti,
  className,
  translationKey,
  items,
  ...rest
}: DropdownWithTranslationProps<IsMulti>) => {
  const { t } = useTranslation()

  const dropdownItems = useMemo(
    () => items.map(x => ({ value: x, label: t(`${translationKey}:${x}`) })),
    [items, t, translationKey],
  )

  return (
    <Dropdown
      isMulti={isMulti}
      items={dropdownItems}
      className={twMerge("placeholder-gray-500", className)}
      {...rest}
    />
  )
}

export const DropdownWithTranslation = DropdownWithTranslationInner as <IsMulti extends boolean>(
  props: DropdownWithTranslationProps<IsMulti>,
) => JSX.Element
