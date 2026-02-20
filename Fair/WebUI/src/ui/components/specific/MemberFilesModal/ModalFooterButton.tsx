import { TFunction } from "i18next"
import { twMerge } from "tailwind-merge"

import { SvgInfoCircle, SvgTrashSm } from "assets"
import { Tooltip } from "ui/components"

export type ModalFooterButtonBaseProps = {
  t: TFunction
  onRemoveClick?: () => void
}

export type ModalFooterButtonProps = Pick<HTMLButtonElement, "disabled"> & ModalFooterButtonBaseProps

export const ModalFooterButton = ({ disabled, t, onRemoveClick }: ModalFooterButtonProps) => (
  <div className={twMerge("flex items-center gap-2", !disabled && "cursor-pointer")}>
    <button
      className={twMerge("flex items-center gap-2 text-2xs leading-4 text-gray-800")}
      onClick={onRemoveClick}
      disabled={disabled}
    >
      <SvgTrashSm className={twMerge("stroke-gray-800", disabled && "opacity-40")} />
      <span className={twMerge(disabled && "opacity-40")}>{t("removeImage")}</span>
    </button>
    <Tooltip text={t("removeImageTooltip")}>
      <span>
        <SvgInfoCircle className="stroke-gray-800" />
      </span>
    </Tooltip>
  </div>
)
