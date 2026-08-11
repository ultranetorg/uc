import { ButtonHTMLAttributes, memo } from "react"
import { Trans } from "react-i18next"
import { twMerge } from "tailwind-merge"

import { SvgXSm } from "assets"

export type ResetAllButtonProps = Pick<ButtonHTMLAttributes<HTMLButtonElement>, "disabled" | "onClick">

export const ResetAllButton = memo(({ disabled, onClick }: ResetAllButtonProps) => (
  <button
    className={twMerge(
      "flex items-center gap-1 text-xs capitalize leading-3.75 text-gray-750",
      disabled ? "text-gray-400" : "cursor-pointer hover:font-medium",
    )}
    onClick={onClick}
    disabled={disabled}
  >
    <SvgXSm className={twMerge("fill-gray-800", disabled && "fill-gray-400")} />
    <Trans ns="common" i18nKey="resetAll" />
  </button>
))
