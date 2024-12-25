import { isDate, upperFirst } from "lodash"

import { Breakpoints } from "constants"
import { AnalysisResult, Currency } from "types"
import { NegativeTag, PositiveTag, Tag, TextLink, TableColumn, TableItem } from "ui/components"
import { formatDataLength, formatDateTime, formatCurrencyString, shortenWideString } from "utils"

export const getTableItemRenderer =
  (tag?: string, breakpoint?: Breakpoints, currency?: Currency, rateUsd?: number, emissionMultiplier?: number) =>
  (column: TableColumn) => {
    // New
    if (column.type === "address") {
      return (_: any, item: TableItem) => (
        <TextLink to={`/authors/${item.address.author}/resources/${item.address.resource}`}>
          {item.address.author + "/" + item.address.resource}
        </TextLink>
      )
    }

    // Old
    if (column.accessor === "id") {
      return (value: string) => {
        const path = tag === "transactions" ? `/transactions/${value}` : `/operations/${value}`
        return <TextLink to={path}>{value}</TextLink>
      }
    }

    if (column.accessor === "address.resource") {
      return (value: any, item: TableItem) => (
        <TextLink to={`/authors/${item.author}/resources/${value}`}>{value}</TextLink>
      )
    }

    if (column.accessor === "signer") {
      return (value: string, item: TableItem) => <TextLink to={`/accounts/${item.signer}`}>{value}</TextLink>
    }

    if (column.accessor === "bid") {
      return (value: bigint) => formatCurrencyString(value, currency, rateUsd, emissionMultiplier)
    }

    if (column.accessor === "bidBy") {
      return (value: string) => <TextLink to={`/accounts/${value}`}>{shortenWideString(value)}</TextLink>
    }

    if (column.accessor === "analyzer") {
      return (_: unknown, item: TableItem) => {
        const { analyzerName, analyzerAddress } = item

        if (!analyzerName) {
          return <TextLink to={`/accounts/${analyzerAddress}`}>{analyzerAddress}</TextLink>
        }

        return (
          <div className="flex overflow-hidden text-ellipsis whitespace-nowrap">
            {analyzerName} (<TextLink to={`/accounts/${analyzerAddress}`}>{analyzerAddress}</TextLink>)
          </div>
        )
      }
    }

    if (column.accessor === "result") {
      const isMediumView =
        breakpoint === Breakpoints.sm || breakpoint === Breakpoints.xs || breakpoint === Breakpoints.md
      return (value: AnalysisResult) => {
        if (value === "Positive") {
          return (
            <div className="flex items-center justify-center">
              <PositiveTag hideLabel={isMediumView} />
            </div>
          )
        }

        if (value === "Negative") {
          return (
            <div className="flex items-center justify-center">
              <NegativeTag hideLabel={isMediumView} />
            </div>
          )
        }

        return (
          <div className="flex items-center justify-center">
            <Tag>{value}</Tag>
          </div>
        )
      }
    }

    if (column.accessor === "data.length") {
      return (value: number) => formatDataLength(value)
    }

    if (column.accessor === "$type") {
      return (value: string) => upperFirst(value)
    }

    if (column.accessor === "description" || column.accessor === "data.value" || column.accessor === "data.type") {
      return (value: string) => (
        <div title={value} className="overflow-hidden text-ellipsis">
          {value}
        </div>
      )
    }

    return (value: any) => (!isDate(value) ? value : formatDateTime(value))
  }
