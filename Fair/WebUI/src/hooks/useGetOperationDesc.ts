import { camelCase } from "lodash"
import { useCallback } from "react"
import { useTranslation } from "react-i18next"

import { Currency, Operation, AuthorRegistration } from "types"
import { formatCurrencyString, shortenString } from "utils"

export const useGetOperationDesc = () => {
  const { t } = useTranslation("operations")

  const getOperationDesc = useCallback(
    (operation: Operation, currency?: Currency, rateUsd?: number, emissionMultiplier?: number) => {
      if (operation.$type === "authorRegistration") {
        const registration = operation as AuthorRegistration
        return t(camelCase(operation.$type), { count: registration.years, ...registration })
      } else if (
        operation.$type === "authorBid" ||
        operation.$type === "authorTransfer" ||
        operation.$type === "candidacyDeclaration" ||
        operation.$type === "emission" ||
        operation.$type === "resourceCreation" ||
        operation.$type === "resourceUpdation" ||
        operation.$type === "untTransfer"
      ) {
        const obj = operation as any
        const amount = formatCurrencyString(obj.amount, currency, rateUsd, emissionMultiplier)
        const bail = formatCurrencyString(obj.bail, currency, rateUsd, emissionMultiplier)
        const bid = formatCurrencyString(obj.bid, currency, rateUsd, emissionMultiplier)
        const fee = formatCurrencyString(obj.fee, currency, rateUsd, emissionMultiplier)
        const wei = formatCurrencyString(obj.wei, currency, rateUsd, emissionMultiplier)
        const signer = shortenString(obj.signer)
        const release = shortenString(obj.release)
        const to = shortenString(obj.to)

        return t(camelCase(operation.$type), { ...obj, ...{ amount, bail, bid, fee, wei, signer, to, release } })
      }

      return t(camelCase(operation.$type), operation)
    },
    [t],
  )

  return { getOperationDesc }
}
