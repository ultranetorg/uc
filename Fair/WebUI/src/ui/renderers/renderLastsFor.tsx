import { TFunction } from "i18next"

import { getLastsFor } from "utils"

const FONT_SM_CLASSNAME = "text-sm leading-4.25"

export const renderLastsFor = (t: TFunction, creationTime: number) => {
  const formatted = getLastsFor(t, creationTime)
  return (
    <span className={FONT_SM_CLASSNAME} title={formatted}>
      {formatted}
    </span>
  )
}
