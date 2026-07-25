import { memo, useEffect, useMemo } from "react"
import { TFunction } from "i18next"
import {
  Controller,
  useFieldArray,
  UseFormClearErrors,
  useFormContext,
  UseFormSetError,
  useWatch,
} from "react-hook-form"

import { CREATE_PROPOSAL_SINGLE_OPTION_OPERATION_TYPES } from "constants/"
import { CreateProposalData, CreateProposalDataOption, OperationType } from "types"
import { ValidationWrapper } from "ui/components"
import { useCreateProposalContext } from "ui/views"

import { getEditorOperationsFields } from "./constants"
import { OptionsEditorList } from "./OptionsEditorList"
import { renderByParameterValueType } from "./renderers"
import { validateStoreAuthorsRemoval, validateStoreModeratorChange, validateStoreInfoUpdation } from "./validations"

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
  "store-author-addition": validateStoreAuthorsRemoval,
  "store-authors-removal": validateStoreAuthorsRemoval,
  "store-moderator-addition": validateStoreModeratorChange,
  "store-moderator-removal": validateStoreModeratorChange,
  "store-info-updation": validateStoreInfoUpdation,
}

export type OptionsEditorProps = {
  t: TFunction
  labelClassName: string
  isVotingRequired: boolean
}

export const OptionsEditor = memo(({ t, labelClassName, isVotingRequired }: OptionsEditorProps) => {
  const { lastEditedOptionIndex } = useCreateProposalContext()
  const { control, clearErrors, setError } = useFormContext<CreateProposalData>()
  const { fields, append, remove } = useFieldArray<CreateProposalData>({ control, name: "options" })
  const type = useWatch({ control, name: "type" })
  const options = useWatch({ control, name: "options" })

  const operationField = useMemo(() => {
    const fields = getEditorOperationsFields(t)
    return fields?.find(x => x.operationType === type)
  }, [t, type])

  const isSingleOptionProposal =
    !isVotingRequired || CREATE_PROPOSAL_SINGLE_OPTION_OPERATION_TYPES.includes(type as OperationType)

  useEffect(() => {
    if (lastEditedOptionIndex === undefined || type === undefined) return

    clearErrors()
    validationMap[type]?.(t, options, clearErrors, setError, lastEditedOptionIndex)
  }, [clearErrors, lastEditedOptionIndex, options, setError, t, type])

  return (
    <>
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
      {!!operationField?.fields && (
        <>
          <div className="flex flex-col gap-4">
            {!isSingleOptionProposal && (
              <span className="text-2xl font-semibold leading-6 first-letter:uppercase">{t("common:options")}</span>
            )}
            <OptionsEditorList
              t={t}
              singleOption={isSingleOptionProposal}
              operationFields={operationField!}
              fields={fields}
              append={append}
              remove={remove}
            />
          </div>
        </>
      )}
    </>
  )
})
