import { memo, useCallback, useEffect, useMemo, useState } from "react"
import { TFunction } from "i18next"

import { SvgPlusSm } from "assets"
import { OperationType } from "types"
import { ButtonOutline, DebugPanel } from "ui/components"

import { getEditorOperationsFields } from "./constants"
import { OptionEditor } from "./OptionEditor"
import { EditorData } from "./types"

const MAX_OPTIONS_COUNT = 16

export type OptionsEditorListProps = {
  t: TFunction
  operationType?: OperationType
  singleOption: boolean
}

export const OptionsEditorList = memo(({ t, operationType, singleOption }: OptionsEditorListProps) => {
  const [editorData, setData] = useState<EditorData[]>([{ title: "", data: {} }])

  const editorFields = useMemo(() => getEditorOperationsFields(t), [t])
  const currentEditorFields = editorFields?.find(x => x.type === operationType)

  const addDisabled = editorData.length >= MAX_OPTIONS_COUNT

  const onAddClick = useCallback(() => setData(p => [...p, { title: "", data: {} }]), [])

  const onRemoveClick = useCallback((index: number) => setData(prev => prev.filter((_, i) => i !== index)), [])

  useEffect(() => {
    if (singleOption) {
      setData(p => [p[0]])
    }
  }, [singleOption])

  return (
    <div className="flex flex-col gap-4">
      {editorData.map((p, i) => (
        <OptionEditor
          key={i}
          data={p}
          editorTitle={!singleOption ? t("optionEditorTitle", { number: i + 1 }) : undefined}
          editorFields={currentEditorFields}
          onDataChange={(name, value) =>
            setData(p => p.map((item, j) => (i === j ? { ...item, data: { ...item.data, [name]: value } } : item)))
          }
          onTitleChange={title => setData(prev => prev.map((item, j) => (i === j ? { ...item, title } : item)))}
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
            {!addDisabled ? t("addCount", { count: MAX_OPTIONS_COUNT - editorData.length }) : t("addDisabled")}
          </span>
        </>
      )}

      <DebugPanel data={editorData} />
    </div>
  )
})
