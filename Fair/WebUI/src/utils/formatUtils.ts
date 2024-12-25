import { GROUP_DELIMITER, DECIMAL_DELIMITER } from "constants"
import { Currency } from "types"

import { attosToUnt, attosToUsd } from "utils"

const nonZeroNumber = /[1-9]/g

export const formatDateTime = (date?: Date): string | undefined => {
  return date ? date.toDateString() : undefined
}

export const formatDataLength = (dataLength?: number): string | undefined => {
  return dataLength !== undefined ? dataLength.toLocaleString() + " bytes" : undefined
}

const formatNumber = (num: number | bigint, options?: Intl.NumberFormatOptions): string => {
  const numberFormat = Intl.NumberFormat("en-US", options)
  const parts = numberFormat.formatToParts(num)
  return parts
    .map(part => (part.type === "group" ? GROUP_DELIMITER : part.type === "decimal" ? DECIMAL_DELIMITER : part.value))
    .join("")
}

const formatCurrency = (value: number | bigint, currency?: string): string | undefined => {
  const options = {
    maximumFractionDigits: 2,
    style: !!currency ? "currency" : undefined,
    currency,
  }

  if (value >= 1) {
    return formatNumber(value, options)
  }

  const str = formatNumber(value, {
    ...options,
    maximumFractionDigits: 20,
  })
  const index = str.search(nonZeroNumber)
  return index !== -1 ? str.substring(0, index + 1) : formatNumber(0, options)
}

const formatUnt = (value: number | bigint): string => {
  return `${formatCurrency(value)} UNT`
}

export const formatCurrencyString = (
  attos?: number | bigint,
  currency?: Currency,
  rate?: number,
  multiplier?: number,
): string | undefined => {
  if (currency === "USD" && !!rate && !!multiplier) {
    const usdPrice = attosToUsd(attos, rate, multiplier)
    return formatCurrency(usdPrice!, "USD")
  }

  const untPrice = attosToUnt(attos)
  return formatUnt(untPrice!)
}

export const toYesNo = (value: boolean) => (value === true ? "Yes" : "No")
