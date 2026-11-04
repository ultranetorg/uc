import { memo, ReactNode } from "react"
import { useTranslation } from "react-i18next"
import { Link } from "react-router-dom"
import { twMerge } from "tailwind-merge"

import { useStoreContext, useStorePoliciesContext, useStoreRolesContext } from "app"
import { SvgLock } from "assets"
import { useGetPolicySurveys } from "entities"
import { useResolveStoreId } from "hooks"
import { OperationType, PropsWithClassName, StoreApprovalPolicyChange } from "types"
import { Tooltip } from "ui/components"
import { calculateVotesRequiredToWinProposal, findPolicy, isReferendum, routes } from "utils"

type InfoCellProps = {
  label: string
  locked?: boolean
  lockedTooltip?: string
  to?: string
  children: ReactNode
}

const InfoCell = ({ label, locked, lockedTooltip, to, children }: InfoCellProps) => (
  <>
    <th className="whitespace-nowrap py-1 pr-4 text-left font-normal text-gray-500">{label}</th>
    <td className="whitespace-nowrap py-1 pr-18 text-left font-medium text-gray-800 last:pr-0">
      <div className="flex items-center gap-1">
        {locked && (
          <Tooltip text={lockedTooltip!}>
            <SvgLock className="size-[14px] shrink-0 cursor-default fill-gray-800" />
          </Tooltip>
        )}
        {to ? (
          <Link to={to} className="hover:underline">
            {children}
          </Link>
        ) : (
          children
        )}
      </div>
    </td>
  </>
)

type OperationPolicyInfoBaseProps = {
  operation: OperationType
  title?: string
}

export type OperationPolicyInfoProps = PropsWithClassName & OperationPolicyInfoBaseProps

/**
 * Shows how the operation is created and approved in the store. Visible to moderators and publishers only.
 */
export const OperationPolicyInfo = memo(({ className, operation, title }: OperationPolicyInfoProps) => {
  const { t } = useTranslation("operationPolicyInfo")

  const storeId = useResolveStoreId()
  const { store } = useStoreContext()
  const policies = useStorePoliciesContext()
  const { isModerator, isPublisher } = useStoreRolesContext()

  const { data: surveys } = useGetPolicySurveys(storeId)

  const survey = surveys?.find(
    x => (x.options?.[0]?.operation as StoreApprovalPolicyChange | undefined)?.operation === operation,
  )
  // The policy can be changed by a survey only if its approval is variable.
  const surveyPath =
    survey?.isVariableApproval === true ? routes.moderation.policySurveyDetails(storeId!, survey.id) : undefined
  const isPolicyLocked = survey !== undefined && !survey.isVariableApproval

  const policy = findPolicy(operation, policies)
  if (!store || !policy || (!isModerator && !isPublisher)) return null

  const referendum = isReferendum(operation, policies)
  const votesRequired = calculateVotesRequiredToWinProposal(operation, store, policies)
  const creators = policy.creators.filter(x => x !== "none").map(x => t(`roles:${x}`))

  const membersPath = referendum
    ? routes.moderation.publishers(storeId!, "publishers")
    : routes.moderation.moderators(storeId!, "moderators")

  return (
    <div className={twMerge("flex w-full items-start justify-between gap-8 rounded-lg text-2xs leading-4", className)}>
      {title && <span className="text-2sm font-semibold leading-5 text-gray-800">{title}</span>}
      <table className="ml-auto border-collapse">
        <tbody>
          <tr>
            <InfoCell label={t("labels:creationPolicy")} to={surveyPath}>
              {creators.length > 0 ? creators.join(", ") : "-"}
            </InfoCell>
            <InfoCell label={referendum ? t("labels:publishers") : t("labels:moderators")} to={membersPath}>
              {referendum ? store.authorsIds.length : store.moderatorsIds.length}
            </InfoCell>
          </tr>
          <tr>
            <InfoCell
              label={t("labels:approvalPolicy")}
              locked={isPolicyLocked}
              lockedTooltip={t("policySurveysPage:unchangeablePolicy")}
              to={surveyPath}
            >
              {t(`approvalRequirement:${policy.approval}`)}
            </InfoCell>
            <InfoCell label={t("labels:votesRequired")}>{votesRequired >= 0 ? votesRequired : "-"}</InfoCell>
          </tr>
        </tbody>
      </table>
    </div>
  )
})
