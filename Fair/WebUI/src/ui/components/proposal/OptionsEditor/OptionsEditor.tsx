import { memo, useCallback, useEffect, useMemo, useState } from "react"
import { TFunction } from "i18next"

import { useModerationContext } from "app"
import {
  CREATE_PROPOSAL_HIDDEN_OPERATION_TYPES,
  CREATE_PROPOSAL_OPERATION_TYPES,
  CREATE_PROPOSAL_SINGLE_OPTION_OPERATION_TYPES,
} from "constants/"
import { OperationType, ProposalType } from "types"
import { Dropdown, DropdownItem, MessageBox } from "ui/components"

import { getEditorOperationsFields } from "./constants"
import { OptionsEditorList } from "./OptionsEditorList"
import { renderByParameterValueType } from "./renderers"
import { EditorOperationFields } from "./types"

export type OptionsEditorProps = {
  t: TFunction
  proposalType: ProposalType
  labelClassName: string
  requiresVoting: boolean
}

export const OptionsEditor = memo(({ t, proposalType, labelClassName, requiresVoting }: OptionsEditorProps) => {
  const { data, setData } = useModerationContext()

  const [operationField, setOperationField] = useState<EditorOperationFields | undefined>(undefined)

  const singleOption =
    !requiresVoting ||
    (!!data.type && CREATE_PROPOSAL_SINGLE_OPTION_OPERATION_TYPES.includes(data.type as OperationType))
  const isHiddenType = CREATE_PROPOSAL_HIDDEN_OPERATION_TYPES.includes(data.type as OperationType)

  const typesItems = useMemo<DropdownItem[]>(
    () => CREATE_PROPOSAL_OPERATION_TYPES.map(x => ({ label: t(`operations:${x}`), value: x as string })),
    [t],
  )

  const hiddenTypeItem = useMemo<DropdownItem[] | undefined>(
    () => (isHiddenType ? [{ label: t(`operations:${data.type}`), value: data.type! }] : undefined),
    [data.type, isHiddenType, t],
  )

  const fields = useMemo(() => getEditorOperationsFields(t), [t])

  const handleDataChange = useCallback(
    (name: string, value: string) => setData(p => ({ ...p, [name]: value })),
    [setData],
  )

  const handleTypeChange = useCallback(
    (item: DropdownItem) => {
      const type = item.value as OperationType

      const field = fields?.find(x => x.operationType === type)

      const options = field?.fields?.length ? [{ title: "" }] : undefined
      setData(p => ({
        ...p,
        ...{ type: type, title: p.title, duration: p.duration, description: p.description!, options: options! },
      }))
      setOperationField(field)
    },
    [fields, setData],
  )

  useEffect(() => {
    if (isHiddenType) {
      const field = fields?.find(x => x.operationType === data.type)
      setOperationField(field)

      const options = field?.fields?.length ? [{ title: "" }] : undefined
      setData(p => ({
        ...p,
        ...{ options: options! },
      }))
    }
  }, [data.type, fields, isHiddenType, setData])

  return (
    <>
      <div className="flex flex-col gap-2">
        <span className={labelClassName}>{t("common:type")}:</span>
        <Dropdown
          isMulti={false}
          isDisabled={isHiddenType}
          controlled={true}
          items={!isHiddenType ? typesItems : hiddenTypeItem}
          onChange={handleTypeChange}
          size="large"
          value={data.type}
          placeholder={t("placeholders:selectProposalType", { proposalType })}
        />
      </div>
      {operationField?.parameterValueType && (
        <div className="flex flex-col gap-2">
          <span className={labelClassName}>{operationField.parameterLabel}:</span>
          {renderByParameterValueType[operationField.parameterValueType](
            operationField,
            data[operationField.parameterName!] as string | undefined,
            handleDataChange,
          )}
        </div>
      )}
      {data.options && (
        <>
          <div className="flex flex-col gap-4">
            {!singleOption && (
              <span className="text-xl font-semibold leading-6 first-letter:uppercase">{t("common:options")}:</span>
            )}
            <OptionsEditorList t={t} singleOption={singleOption} operationFields={operationField!} />
          </div>
        </>
      )}
      {data.type && requiresVoting && <MessageBox message={t("addedAnswers")} type="warning" />}
    </>
  )
})
