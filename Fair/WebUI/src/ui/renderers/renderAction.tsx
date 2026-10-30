import { TFunction } from "i18next"

import { OperationType } from "types"

export const renderAction = (t: TFunction, operationType: OperationType) => {
  const value = t(`operations:${operationType}`)
  return (
    <div title={value} className="truncate">
      {value}
    </div>
  )
}

export const renderActionShort = (t: TFunction, operationType: OperationType) => (
  <span title={t(`operations:${operationType}`)}>{t(`operationsShort:${operationType}`)}</span>
)
