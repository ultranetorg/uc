import { memo, useCallback } from "react"
import { TFunction } from "i18next"

import { useModerationContext } from "app"
import { SvgPlusSm } from "assets"
import { ButtonOutline } from "ui/components"

import { OptionEditor } from "./OptionEditor"
import { EditorOperationFields } from "./types"

const MAX_OPTIONS_COUNT = 16

export type OptionsEditorListProps = {
  t: TFunction
  singleOption: boolean
  operationFields: EditorOperationFields
}

export const OptionsEditorList = memo(({ t, singleOption, operationFields }: OptionsEditorListProps) => {
  const { data, setData, setDataOption } = useModerationContext()

  const addDisabled = data.options!.length >= MAX_OPTIONS_COUNT

  const onAddClick = useCallback(
    () => setData(p => ({ ...p, ...{ options: [...data.options!, { title: "" }] } })),
    [data.options, setData],
  )

  const onRemoveClick = useCallback(
    (index: number) => setData(p => ({ ...p, ...{ options: p.options!.filter((_, i) => i !== index) } })),
    [setData],
  )

  return (
    <div className="flex flex-col gap-4">
      {data.options!.map((optionData, i) => (
        <OptionEditor
          key={i}
          t={t}
          data={optionData}
          editorTitle={!singleOption ? t("optionEditorTitle", { number: i + 1 }) : undefined}
          editorFields={operationFields}
          onDataChange={(name, value) => setDataOption(i, p => ({ ...p, [name]: value }))}
          onRemoveClick={i > 0 ? () => onRemoveClick(i) : undefined}
        />
      ))}

      {!singleOption && (
        <>
          <ButtonOutline
            className="h-11 w-full"
            disabled={addDisabled}
            label={t("addOption")}
            iconBefore={<SvgPlusSm className={!addDisabled ? "fill-gray-800" : "fill-gray-400"} />}
            onClick={onAddClick}
          />
          <span className="w-full text-center text-2xs font-medium leading-4">
            {!addDisabled ? t("addCount", { count: MAX_OPTIONS_COUNT - data.options!.length }) : t("addDisabled")}
          </span>
        </>
      )}
    </div>
  )
})
