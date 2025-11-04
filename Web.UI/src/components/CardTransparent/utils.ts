import { get } from "lodash"

import { CardTransparentRow, CardTransparentValueRenderer } from "./types"

export const getValue = (
  accessor: string,
  items: any[],
  row: CardTransparentRow,
  valueRenderer?: CardTransparentValueRenderer,
) => {
  const value = get(items, accessor)
  if (value === undefined) {
    return undefined
  }

  const renderedValue = valueRenderer && valueRenderer(value, row)
  return renderedValue !== undefined ? renderedValue : value.toString()
}
