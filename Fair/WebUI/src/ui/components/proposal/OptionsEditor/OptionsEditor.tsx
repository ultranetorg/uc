import { memo, useEffect, useMemo, useState } from "react"
import { TFunction } from "i18next"
import { Controller, useFieldArray, useFormContext } from "react-hook-form"

import {
  CREATE_PROPOSAL_HIDDEN_OPERATION_TYPES,
  CREATE_PROPOSAL_OPERATION_TYPES,
  CREATE_PROPOSAL_SINGLE_OPTION_OPERATION_TYPES,
} from "constants/"
import { CreateProposalData, OperationType, ProposalType } from "types"
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
  const { control, unregister, watch } = useFormContext<CreateProposalData>()
  const { fields, append, remove, replace } = useFieldArray<CreateProposalData>({ control, name: "options" })
  const type = watch("type")

  const [operationField, setOperationField] = useState<EditorOperationFields | undefined>(undefined)

  const singleOption =
    !requiresVoting || (!!type && CREATE_PROPOSAL_SINGLE_OPTION_OPERATION_TYPES.includes(type as OperationType))
  const isHiddenType = CREATE_PROPOSAL_HIDDEN_OPERATION_TYPES.includes(type as OperationType)

  const typesItems = useMemo<DropdownItem[]>(
    () => CREATE_PROPOSAL_OPERATION_TYPES.map(x => ({ label: t(`operations:${x}`), value: x as string })),
    [t],
  )

  const hiddenTypeItem = useMemo<DropdownItem[] | undefined>(
    () => (isHiddenType ? [{ label: t(`operations:${type}`), value: type! }] : undefined),
    [type, isHiddenType, t],
  )

  const operationFields = useMemo(() => getEditorOperationsFields(t), [t])

  useEffect(() => {
    const field = operationFields?.find(x => x.operationType === type)

    unregister("categoryId")

    if (field?.fields?.length) {
      replace([{ title: "" }])
    } else {
      unregister("options")
      remove()
    }

    setOperationField(field)
  }, [operationFields, remove, replace, type, unregister])

  // useEffect(() => {
  //   if (isHiddenType) {
  //     const field = operationFields?.find(x => x.operationType === type)
  //     setOperationField(field)

  //     const options = field?.fields?.length ? [{ title: "" }] : undefined
  //     setData(p => ({
  //       ...p,
  //       ...{ options: options! },
  //     }))
  //   }
  // }, [type, operationFields, isHiddenType, setData])

  return (
    <>
      <div className="flex flex-col gap-2">
        <span className={labelClassName}>{t("common:type")}:</span>
        <Controller
          control={control}
          name="type"
          rules={{ required: t("validation:requiredType") }}
          render={({ field }) => (
            <Dropdown
              isMulti={false}
              isDisabled={isHiddenType}
              controlled={true}
              items={!isHiddenType ? typesItems : hiddenTypeItem}
              onChange={item => field.onChange(item.value)}
              size="large"
              value={field.value}
              placeholder={t("placeholders:selectProposalType", { proposalType })}
            />
          )}
        />
      </div>
      {operationField?.parameterValueType && (
        <div className="flex flex-col gap-2">
          <span className={labelClassName}>{operationField.parameterLabel}:</span>
          <Controller
            control={control}
            name={operationField.parameterName!}
            rules={{ required: true }}
            render={({ field }) =>
              renderByParameterValueType[operationField!.parameterValueType!](
                operationField,
                field.value as string | undefined,
                field.onChange,
              )
            }
          />
        </div>
      )}
      {fields.length > 0 && (
        <>
          <div className="flex flex-col gap-4">
            {!singleOption && (
              <span className="text-xl font-semibold leading-6 first-letter:uppercase">{t("common:options")}:</span>
            )}
            <OptionsEditorList
              t={t}
              singleOption={singleOption}
              operationFields={operationField!}
              fields={fields}
              append={append}
              remove={remove}
            />
          </div>
        </>
      )}
      {type && requiresVoting && <MessageBox message={t("addedAnswers")} type="warning" />}
    </>
  )
})
