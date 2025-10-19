import { SvgCheckSquareSmSvg } from "assets"
import { useCallback, useState } from "react"

import { ButtonPrimary, RadioCard } from "ui/components"

const ALTERNATIVE_OPTION_ITEMS = [
  { title: "Any", description: "Подходит любой вариант" },
  { title: "Neither", description: "Ни один из вариантов не подходит" },
  { title: "Ban", description: "Против проведения референдума" },
  { title: "Banish", description: "Бан автора данного референдума" },
]

export const AlternativeOptions = () => {
  const [checkedIndex, setCheckedIndex] = useState(-1)

  const handleClick = useCallback((index: number) => setCheckedIndex(p => (p !== index ? index : -1)), [])

  return (
    <div className="flex flex-col gap-6 rounded-lg border border-gray-300 bg-ivory p-4 text-gray-800">
      <div className="flex flex-col gap-2">
        <span className="text-2base font-semibold leading-5.25">Альтернативные варианты</span>
        <span className="text-2xs font-medium leading-4 text-gray-500">
          Если ни один из предложенных вариантов вам не подходит
        </span>
      </div>
      <div className="grid grid-cols-2 gap-2">
        {ALTERNATIVE_OPTION_ITEMS.map((x, index) => (
          <RadioCard
            key={index}
            className="w-full"
            title={x.title}
            description={x.description}
            checked={index === checkedIndex}
            onClick={() => handleClick(index)}
          />
        ))}
      </div>
      <ButtonPrimary
        className="h-11 w-37.5 self-end"
        label="Vote"
        iconAfter={<SvgCheckSquareSmSvg className="fill-white" />}
      />
    </div>
  )
}
