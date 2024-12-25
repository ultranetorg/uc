import { isDate } from "lodash"

import { Currency } from "types"
import { CardListRow } from "ui/components"
import { formatDateTime, formatCurrencyString, makeKey } from "utils"

const years = [1, 5, 10]
const authorLengths = [1, 5, 10, 15]

export const getCardListValueRenderer = (currency?: Currency, rate?: number, emissionMultiplier?: number) => {
  return (row: CardListRow, value: any) => {
    if (row.type === "currency") {
      return formatCurrencyString(value, currency, rate, emissionMultiplier)
    }

    if (row.accessor === "rentResource" || row.accessor === "rentResourceData") {
      return (
        <>
          {years.map((year, index) => (
            <div className="flex flex-row justify-between gap-4" key={year}>
              <div>{year} years</div>
              <div>{formatCurrencyString(value[index], currency, rate, emissionMultiplier)}</div>
            </div>
          ))}
        </>
      )
    }
    if (row.accessor === "rentAuthor") {
      return (
        <table className="w-full">
          <thead>
            <tr>
              <td></td>
              <td>1 char</td>
              <td>5 chars</td>
              <td>10 chars</td>
              <td>15 chars</td>
            </tr>
          </thead>
          <tbody>
            {years.map((year, i) => (
              <tr>
                <td>{year} years</td>
                {authorLengths.map((length, j) => (
                  <td key={makeKey(year, length)}>
                    {formatCurrencyString(value[i][j], currency, rate, emissionMultiplier)}
                  </td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      )
    }

    if (row.accessor === "operationsPerSecond" || row.accessor === "transactionsPerSecond") {
      return value === 0 ? <div className="select-none">Estimating...</div> : value
    }

    return !isDate(value) ? value.toString() : formatDateTime(value)
  }
}
