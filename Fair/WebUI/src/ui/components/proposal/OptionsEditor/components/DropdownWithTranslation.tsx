import { TFunction } from "i18next"
import { useMemo } from "react"
import { PropsWithClassName } from "types"
import { Dropdown, DropdownProps } from "ui/components"

type DropdownWithTranslationBaseProps = {
  t: TFunction
  translationKey: string
  items: string[]
}

export type DropdownWithTranslationProps = PropsWithClassName &
  DropdownWithTranslationBaseProps &
  Pick<DropdownProps, "placeholder" | "onChange">

export const DropdownWithTranslation = ({
  className,
  t,
  translationKey,
  items,
  ...rest
}: DropdownWithTranslationProps) => {
  const dropdownItems = useMemo(
    () => items.map(x => ({ value: x, label: t(`${translationKey}:${x}`) })),
    [items, t, translationKey],
  )

  return <Dropdown items={dropdownItems} className={className} {...rest} />
}
