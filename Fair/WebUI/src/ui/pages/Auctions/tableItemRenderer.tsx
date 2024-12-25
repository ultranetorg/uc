import { isDate } from "lodash"
import { Link } from "react-router-dom"

import { Currency } from "types"
import { TableColumn } from "ui/components"
import { formatCurrencyString, formatDateTime } from "utils"

export const getTableItemRenderer =
  (currency?: Currency, rateUsd?: number, emissionMultiplier?: number) => (column: TableColumn) => {
    if (column.accessor === "name") {
      return (value: string) => <Link to={`/auctions/${value}`}>{value}</Link>
    }
    if (column.accessor === "lastBid") {
      return (value: bigint) => formatCurrencyString(value, currency, rateUsd, emissionMultiplier)
    }

    return (value: any) => (!isDate(value) ? value : formatDateTime(value))
  }
