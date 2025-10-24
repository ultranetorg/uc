import { memo } from "react"
import { twMerge } from "tailwind-merge"
import { TFunction } from "i18next"
import { Controller, useFormContext } from "react-hook-form"

import { useModerationContext } from "app"
import { SvgX } from "assets"
import { CreateProposalData } from "types"
import { Input, ValidationWrapper } from "ui/components"

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
  const {
    control,
    formState: { errors },
  } = useFormContext<CreateProposalData>()
  const { setLastEditedOptionIndex } = useModerationContext()

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
            <ValidationWrapper message={fieldState.error?.message}>
              <Input
                onChange={field.onChange}
                value={field.value}
                className="h-10 placeholder-gray-500"
                placeholder={t("placeholders:enterOptionTitle")}
                error={!!fieldState.error?.message}
              />
            </ValidationWrapper>
          )}
        />
        <ValidationWrapper
          className="flex flex-col gap-2.5"
          message={errors?.options?.[index]?.message as string | undefined}
        >
          {editorFields?.fields?.map(x => (
            <Controller<CreateProposalData>
              key={x.name}
              control={control}
              // @ts-expect-error fix
              name={`options.${index}.${x.name}`}
              rules={{ required: true, ...x.rules }}
              render={({ field, fieldState }) =>
                renderByValueType[x.valueType!]({
                  errorMessage: fieldState.error?.message,
                  field: x,
                  value: field.value as string,
                  onChange: value => {
                    field.onChange(value)
                    setLastEditedOptionIndex(index)
                  },
                })
              }
            />
          ))}
        </ValidationWrapper>
      </div>
    </div>
  )
})
