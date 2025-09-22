import { memo, useCallback } from "react"
import { TFunction } from "i18next"

import { SvgPlusSm } from "assets"
import { CreateProposalData, CreateProposalDataOption } from "types"
import { ButtonOutline } from "ui/components"

import { OptionEditor } from "./OptionEditor"
import { EditorOperationFields } from "./types"

const MAX_OPTIONS_COUNT = 16

export type OptionsEditorListProps = {
  t: TFunction
  singleOption: boolean
  operationFields: EditorOperationFields
  data: CreateProposalData
  onDataChange: (name: string, value: string | CreateProposalDataOption[] | undefined) => void
  onDataOptionChange: (index: number, name: string, value: string | string[]) => void
}

export const OptionsEditorList = memo(
  ({ t, singleOption, operationFields, data, onDataChange, onDataOptionChange }: OptionsEditorListProps) => {
    const addDisabled = data.options!.length >= MAX_OPTIONS_COUNT

    const onAddClick = useCallback(
      () => onDataChange("options", [...data.options!, { title: "" }]),
      [data.options, onDataChange],
    )

    const onRemoveClick = useCallback(
      (index: number) =>
        onDataChange(
          "options",
          data.options!.filter((_, i) => i !== index),
        ),
      [data.options, onDataChange],
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
            onDataChange={(name, value) => onDataOptionChange(i, name, value)}
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
  },
)
