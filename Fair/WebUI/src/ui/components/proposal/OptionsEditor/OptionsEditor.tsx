import { memo, useEffect, useMemo, useState } from "react"
import { TFunction } from "i18next"
import {
  Controller,
  useFieldArray,
  UseFormClearErrors,
  useFormContext,
  UseFormSetError,
  useWatch,
} from "react-hook-form"

import { useModerationContext } from "app"
import {
  CREATE_DISCUSSION_EXTRA_OPERATION_TYPES,
  CREATE_DISCUSSION_HIDDEN_OPERATION_TYPES,
  CREATE_DISCUSSION_OPERATION_TYPES,
  CREATE_DISCUSSION_SINGLE_OPTION_OPERATION_TYPES,
  CREATE_REFERENDUM_OPERATION_TYPES,
} from "constants/"
import { CreateProposalData, CreateProposalDataOption, OperationType, ProposalType } from "types"
import { Dropdown, DropdownItem, MessageBox, ValidationWrapper } from "ui/components"

import { getEditorOperationsFields } from "./constants"
import { OptionsEditorList } from "./OptionsEditorList"
import { renderByParameterValueType } from "./renderers"
import { EditorOperationFields } from "./types"
import {
  validateSiteAuthorRemoval,
  validateSiteMemberAddition,
  validateSiteModeratorRemoval,
  validateSiteTextChange,
} from "./validations"

const validationMap: Record<
  string,
  (
    t: TFunction,
    options: CreateProposalDataOption[],
    clearErrors: UseFormClearErrors<CreateProposalData>,
    setError: UseFormSetError<CreateProposalData>,
    lastEditedIndex: number,
  ) => void
> = {
  "site-author-addition": validateSiteMemberAddition,
  "site-author-removal": validateSiteAuthorRemoval,
  "site-moderator-addition": validateSiteMemberAddition,
  "site-moderator-removal": validateSiteModeratorRemoval,
  "site-text-change": validateSiteTextChange,
}

export type OptionsEditorProps = {
  t: TFunction
  proposalType: ProposalType
  labelClassName: string
  requiresVoting: boolean
}

export const OptionsEditor = memo(({ t, proposalType, labelClassName, requiresVoting }: OptionsEditorProps) => {
  const { lastEditedOptionIndex } = useModerationContext()
  const { control, clearErrors, setError, unregister } = useFormContext<CreateProposalData>()
  const { fields, append, remove, replace } = useFieldArray<CreateProposalData>({ control, name: "options" })
  const type = useWatch({ control, name: "type" })
  const options = useWatch({ control, name: "options" })

  const [operationField, setOperationField] = useState<EditorOperationFields | undefined>(undefined)

  const discussionSingleOption =
    proposalType === "discussion" &&
    (!requiresVoting || (!!type && CREATE_DISCUSSION_SINGLE_OPTION_OPERATION_TYPES.includes(type as OperationType)))

  const discussionHiddenType =
    proposalType === "discussion" && CREATE_DISCUSSION_HIDDEN_OPERATION_TYPES.includes(type as OperationType)

  const typesItems = useMemo<DropdownItem[]>(() => {
    const types = proposalType === "discussion" ? CREATE_DISCUSSION_OPERATION_TYPES : CREATE_REFERENDUM_OPERATION_TYPES
    return types.map(x => ({
      // @ts-expect-error fix
      label: !CREATE_DISCUSSION_EXTRA_OPERATION_TYPES.includes(x) ? t(`operations:${x}`) : t(`extraOperations:${x}`),
      value: x as string,
    }))
  }, [proposalType, t])

  const hiddenTypeItem = useMemo<DropdownItem[] | undefined>(
    () => (discussionHiddenType ? [{ label: t(`operations:${type}`), value: type! }] : undefined),
    [type, discussionHiddenType, t],
  )

  const operationFields = useMemo(() => getEditorOperationsFields(t), [t])

  useEffect(() => {
    const field = operationFields?.find(x => x.operationType === type)
    setOperationField(field)

    unregister("categoryId")

    if (field?.fields?.length) {
      replace([{ title: "" }])
    } else {
      remove()
    }
  }, [operationFields, remove, replace, type, unregister])

  useEffect(() => {
    if (lastEditedOptionIndex === undefined || type === undefined) return

    clearErrors()
    validationMap[type]?.(t, options, clearErrors, setError, lastEditedOptionIndex)
  }, [clearErrors, lastEditedOptionIndex, options, setError, t, type])

  // useEffect(() => {
  //   if (isHiddenType) {
  //     const field = operationFields?.find(x => x.operationType === type)
  //     setOperationField(field)

  //     const options = field?.fields?.length ? [{ title: "" }] : undefined
  //     setData(p => ({
  //       ...p,
  //       ...{ options: options },
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
              isDisabled={discussionHiddenType}
              controlled={true}
              items={!discussionHiddenType ? typesItems : hiddenTypeItem}
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
            // @ts-expect-error fix
            rules={{ required: true, ...operationField?.parameterRules }}
            render={({ field, fieldState }) => (
              <ValidationWrapper message={operationField?.parameterRules && fieldState.error?.message}>
                {renderByParameterValueType[operationField!.parameterValueType!](
                  operationField,
                  field.value as string | undefined,
                  field.onChange,
                )}
              </ValidationWrapper>
            )}
          />
        </div>
      )}
      {fields.length > 0 && (
        <>
          <div className="flex flex-col gap-4">
            {!discussionSingleOption && (
              <span className="text-2xl font-semibold leading-6 first-letter:uppercase">{t("common:options")}</span>
            )}
            <OptionsEditorList
              t={t}
              singleOption={discussionSingleOption}
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
