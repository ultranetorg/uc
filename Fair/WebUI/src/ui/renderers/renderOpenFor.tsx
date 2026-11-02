import { TFunction } from "i18next"

import { getOpenFor as getOpenFor } from "utils"

const FONT_SM_CLASSNAME = "text-sm leading-4.25"

export const renderOpenFor = (t: TFunction, creationTime: number) => {
  const formatted = getOpenFor(t, creationTime)
  return (
    <span className={FONT_SM_CLASSNAME} title={formatted}>
      {formatted}
    </span>
  )
}
