import { memo } from "react"
import { twMerge } from "tailwind-merge"
import { TFunction } from "i18next"
import { Controller, useFormContext } from "react-hook-form"

import { SvgX } from "assets"
import { CreateProposalData } from "types"
import { Input } from "ui/components"

import { renderByValueType } from "./renderers"
import { EditorOperationFields } from "./types"
import { validateUniqueTitle } from "./validations"

export type OptionEditorProps = {
  index: number
  t: TFunction
  editorTitle?: string
  editorFields?: EditorOperationFields
  onRemoveClick?: () => void
}

export const OptionEditor = memo(({ index, t, editorTitle, editorFields, onRemoveClick }: OptionEditorProps) => {
  const { control } = useFormContext<CreateProposalData>()

  return (
    <div className={twMerge("flex flex-col gap-4 rounded-lg border border-gray-300 p-4")}>
      {(editorTitle || onRemoveClick) && (
        <div className="flex items-center justify-between">
          {editorTitle && <span className="text-2sm font-medium leading-5">{editorTitle}</span>}
          {onRemoveClick && (
            <SvgX className="cursor-pointer stroke-gray-500 hover:stroke-gray-800" onClick={onRemoveClick} />
          )}
        </div>
      )}

      <div className="flex flex-col gap-2.5">
        <Controller
          control={control}
          name={`options.${index}.title`}
          rules={{ required: t("validation:requiredTitle"), validate: validateUniqueTitle(t) }}
          render={({ field, fieldState }) => (
            <Input
              onChange={field.onChange}
              value={field.value}
              className="h-10 placeholder-gray-500"
              placeholder={t("placeholders:enterOptionTitle")}
              error={fieldState.error?.message}
            />
          )}
        />
        {editorFields?.fields?.map(x => (
          <Controller
            key={x.name}
            control={control}
            name={`options.${index}.${x.name}`}
            render={({ field }) => renderByValueType[x.valueType!](x, field.value, field.onChange)}
          />
        ))}
      </div>
    </div>
  )
})
