import { memo } from "react"
import { TFunction } from "i18next"
import { twMerge } from "tailwind-merge"

import { SvgX } from "assets"
import { CreateProposalDataOption } from "types"
import { Input } from "ui/components"

import { renderByValueType } from "./renderers"
import { EditorOperationFields } from "./types"

export type OptionEditorProps = {
  t: TFunction
  editorTitle?: string
  editorFields?: EditorOperationFields
  data: CreateProposalDataOption
  onDataChange: (name: string, value: string | string[]) => void
  onRemoveClick?: () => void
}

export const OptionEditor = memo(
  ({ t, editorTitle, editorFields, data, onDataChange, onRemoveClick }: OptionEditorProps) => (
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
        <Input
          onChange={value => onDataChange("title", value)}
          value={data.title}
          className="h-10 placeholder-gray-500"
          placeholder={t("placeholders:enterOptionTitle")}
        />
        {editorFields?.fields?.map(x => {
          const value = data[x.name]
          return renderByValueType[x.valueType!](t, x, value, onDataChange)
        })}
      </div>
    </div>
  ),
)
