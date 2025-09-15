import { memo } from "react"

import { SvgX } from "assets"
import { ButtonOutline, DropdownSecondary, Input, Textarea } from "ui/components"

import { EditorData, EditorOperationFields } from "./types"
import { toDropdownItems } from "./utils"

export type OptionEditorProps = {
  editorTitle?: string
  editorFields?: EditorOperationFields
  data: EditorData
  onDataChange: (name: string, value: string | string[]) => void
  onTitleChange: (value: string) => void
  onRemoveClick?: () => void
}

export const OptionEditor = memo(
  ({ editorTitle, editorFields, data, onDataChange, onTitleChange, onRemoveClick }: OptionEditorProps) => (
    <div className="flex flex-col gap-4 rounded-lg border border-gray-300 p-4">
      {editorTitle ||
        (onRemoveClick && (
          <div className="flex items-center justify-between">
            {editorTitle && <span className="text-2sm font-medium leading-5">{editorTitle}</span>}
            {onRemoveClick && (
              <SvgX className="cursor-pointer stroke-gray-500 hover:stroke-gray-800" onClick={onRemoveClick} />
            )}
          </div>
        ))}

      <div className="flex flex-col gap-2.5">
        <Input onChange={onTitleChange} value={data.title} className="h-10" />
        {editorFields?.fields.map(x => {
          const value = data.data[x.name]

          if (x.type === "dropdown") {
            return (
              <DropdownSecondary
                key={x.name}
                items={toDropdownItems(x.options)}
                placeholder={x.placeholder}
                onChange={item => onDataChange(x.name, item.value)}
              />
            )
          }

          if (x.type === "input") {
            return (
              <Input
                key={x.name}
                id={x.name}
                className="h-10"
                placeholder={x.placeholder}
                value={value as string}
                onChange={value => onDataChange(x.name, value)}
              />
            )
          }

          if (x.type === "select") {
            return <ButtonOutline key={x.name} className="h-10 w-full" label={x.placeholder!} />
          }

          if (x.type === "textarea") {
            return (
              <Textarea
                key={x.name}
                placeholder={x.placeholder}
                value={value as string}
                onChange={value => onDataChange(x.name, value)}
              />
            )
          }

          return null
        })}
      </div>
    </div>
  ),
)
