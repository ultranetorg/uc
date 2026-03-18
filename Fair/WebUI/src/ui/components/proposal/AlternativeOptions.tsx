import { memo, useCallback, useMemo, useState } from "react"

import { useTranslation } from "react-i18next"
import { SvgCheckSquareSmSvg } from "assets"

import { ButtonPrimary, RadioCard } from "ui/components"
import { SpecialChoice } from "types"

export type AlternativeOptionsProps = {
  votedValue?: number
  onVoteClick: (value: number) => void
}

export const AlternativeOptions = memo(({ votedValue, onVoteClick }: AlternativeOptionsProps) => {
  const { t } = useTranslation("alternativeOptions")

  const [checkedValue, setCheckedValue] = useState(SpecialChoice.Neither)

  const optionItems = useMemo(
    () => [
      {
        title: t("common:neither"),
        description: t(`specialChoice:${String(SpecialChoice.Neither)}`),
        value: SpecialChoice.Neither,
      },
      {
        title: t("common:any"),
        description: t(`specialChoice:${String(SpecialChoice.Any)}`),
        value: SpecialChoice.Any,
      },
      {
        title: t("common:ban"),
        description: t(`specialChoice:${String(SpecialChoice.Ban)}`),
        value: SpecialChoice.Ban,
      },
      {
        title: t("common:banish"),
        description: t(`specialChoice:${String(SpecialChoice.Banish)}`),
        value: SpecialChoice.Banish,
      },
    ],
    [t],
  )

  const handleRadioClick = useCallback(
    (index: number) => {
      // Check if voting not in progress
      if (votedValue === undefined) {
        setCheckedValue(index)
      }
    },
    [votedValue],
  )

  const handleVoteClick = useCallback(() => onVoteClick(checkedValue), [checkedValue, onVoteClick])

  return (
    <div className="flex flex-col gap-6 rounded-lg border border-gray-300 bg-ivory p-4 text-gray-800">
      <div className="flex flex-col gap-2">
        <span className="text-2base font-semibold leading-5.25">{t("title")}</span>
        <span className="text-2xs font-medium leading-4 text-gray-500">{t("description")}</span>
      </div>
      <div className="grid grid-cols-2 gap-2">
        {optionItems.map((x, index) => (
          <RadioCard
            key={index}
            className="w-full"
            title={x.title}
            description={x.description}
            checked={x.value === checkedValue}
            onClick={() => handleRadioClick(x.value)}
          />
        ))}
      </div>
      <ButtonPrimary
        disabled={votedValue !== undefined}
        loading={votedValue !== undefined && votedValue < 0}
        className="h-11 w-37.5 self-end"
        label="Vote"
        iconAfter={<SvgCheckSquareSmSvg className="fill-white" />}
        onClick={handleVoteClick}
      />
    </div>
  )
})
