import { TFunction } from "i18next"
import { useMemo } from "react"
import { PropsWithClassName } from "types"
import { Dropdown, DropdownProps } from "ui/components"

type DropdownWithTranslationBaseProps = {
  t: TFunction
  translationKey: string
  items: string[]
}

export type DropdownWithTranslationProps<IsMulti extends boolean> = PropsWithClassName &
  DropdownWithTranslationBaseProps &
  Pick<DropdownProps<IsMulti>, "isMulti" | "placeholder" | "onChange">

export const DropdownWithTranslationInner = <IsMulti extends boolean>({
  isMulti,
  className,
  t,
  translationKey,
  items,
  ...rest
}: DropdownWithTranslationProps<IsMulti>) => {
  const dropdownItems = useMemo(
    () => items.map(x => ({ value: x, label: t(`${translationKey}:${x}`) })),
    [items, t, translationKey],
  )

  return <Dropdown isMulti={isMulti} items={dropdownItems} className={className} {...rest} />
}

export const DropdownWithTranslation = DropdownWithTranslationInner as <IsMulti extends boolean>(
  props: DropdownWithTranslationProps<IsMulti>,
) => JSX.Element
