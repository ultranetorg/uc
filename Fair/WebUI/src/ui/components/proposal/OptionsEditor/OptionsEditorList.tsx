import { memo, useCallback } from "react"
import { TFunction } from "i18next"
import { FieldArrayWithId, UseFieldArrayAppend, UseFieldArrayRemove } from "react-hook-form"

import { SvgPlusSm } from "assets"
import { CreateProposalData } from "types"
import { ButtonOutline } from "ui/components"

import { OptionEditor } from "./OptionEditor"
import { EditorOperationFields } from "./types"

const MAX_OPTIONS_COUNT = 16

export type OptionsEditorListProps = {
  t: TFunction
  singleOption: boolean
  operationFields: EditorOperationFields
  fields: FieldArrayWithId<CreateProposalData, "options" | `options.${number}.candidatesIds`, "id">[]
  append: UseFieldArrayAppend<CreateProposalData>
  remove: UseFieldArrayRemove
}

export const OptionsEditorList = memo(
  ({ t, singleOption, operationFields, fields, append, remove }: OptionsEditorListProps) => {
    const addDisabled = fields.length >= MAX_OPTIONS_COUNT

    const onAddClick = useCallback(() => append({ title: "" }), [append])

    const onRemoveClick = useCallback((index: number) => remove(index), [remove])

    return (
      <div className="flex flex-col gap-4">
        {fields.map((field, i) => (
          <OptionEditor
            key={field.id}
            index={i}
            t={t}
            editorTitle={!singleOption ? t("optionEditorTitle", { number: i + 1 }) : undefined}
            editorFields={operationFields}
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
              {!addDisabled ? t("addCount", { count: MAX_OPTIONS_COUNT - fields.length }) : t("addDisabled")}
            </span>
          </>
        )}
      </div>
    )
  },
)
