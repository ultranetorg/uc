import { TFunction } from "i18next"
import { twMerge } from "tailwind-merge"

import { SvgTrashSm } from "assets"

export type ModalFooterButtonBaseProps = {
  t: TFunction
  onRemoveClick?: () => void
}

export type ModalFooterButtonProps = Pick<HTMLButtonElement, "disabled"> & ModalFooterButtonBaseProps

export const ModalFooterButton = ({ disabled, t, onRemoveClick }: ModalFooterButtonProps) => (
  <button
    className={twMerge("flex items-center gap-2 text-2xs leading-4 text-gray-800", !disabled && "cursor-pointer")}
    onClick={onRemoveClick}
    disabled={disabled}
  >
    <SvgTrashSm className={twMerge("stroke-gray-800", disabled && "opacity-40")} />
    <span className={twMerge(disabled && "opacity-40")}>{t("removeImage")}</span>
    {/* <SvgInfoCircle className="stroke-gray-800" /> // TODO: add button with tooltip */}
  </button>
)
