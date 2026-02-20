import { TFunction } from "i18next"

import { ButtonPrimary } from "ui/components"

import { ModalFooterButton } from "./ModalFooterButton"

export type ModalFooterProps = {
  t: TFunction
  refs: number
  onRemoveClick?: () => void
  onSelectClick?: () => void
}

export const ModalFooter = ({ t, refs, onRemoveClick, onSelectClick }: ModalFooterProps) => (
  <div className="flex items-center justify-between border-t border-t-gray-300 bg-gray-200 px-6 py-4">
    <ModalFooterButton t={t} onRemoveClick={onRemoveClick} disabled={refs !== 0} />
    <ButtonPrimary className="h-9 capitalize" label={t("common:select")} onClick={onSelectClick} />
  </div>
)
