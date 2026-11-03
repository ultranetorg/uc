import { memo, ReactNode, useCallback } from "react"
import { useTranslation } from "react-i18next"
import { useNavigate } from "react-router-dom"
import { twMerge } from "tailwind-merge"

import { useStoreContext, useStorePoliciesContext, useStoreRolesContext } from "app"
import { SvgLock } from "assets"
import { useGetPolicySurveys } from "entities"
import { useResolveStoreId } from "hooks"
import { OperationType, PropsWithClassName, StoreApprovalPolicyChange } from "types"
import { Tooltip } from "ui/components"
import { calculateVotesRequiredToWinProposal, findPolicy, isReferendum, routes } from "utils"

type InfoItemProps = {
  label: string
  children: ReactNode
}

const InfoItem = ({ label, children }: InfoItemProps) => (
  <div className="flex items-center gap-1 whitespace-nowrap">
    <span className="text-gray-500">{label}:</span>
    <span className="font-medium text-gray-800">{children}</span>
  </div>
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
  const navigate = useNavigate()
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
  const isPolicyChangeable = survey?.isVariableApproval === true
  const isPolicyLocked = survey !== undefined && !survey.isVariableApproval

  const handleClick = useCallback(() => {
    if (survey && isPolicyChangeable) navigate(routes.moderation.policySurveyDetails(storeId!, survey.id))
  }, [survey, isPolicyChangeable, navigate, storeId])

  const policy = findPolicy(operation, policies)
  if (!store || !policy || (!isModerator && !isPublisher)) return null

  const referendum = isReferendum(operation, policies)
  const votesRequired = calculateVotesRequiredToWinProposal(operation, store, policies)
  const creators = policy.creators.filter(x => x !== "none").map(x => t(`roles.${x}`))

  return (
    <div
      className={twMerge(
        "flex w-full items-center justify-between gap-8 rounded-lg border border-gray-300 bg-gray-100 px-6 py-4 text-2xs leading-4",
        isPolicyChangeable && "cursor-pointer hover:border-gray-400",
        className,
      )}
      onClick={handleClick}
    >
      {title && <span className="text-2sm font-semibold leading-5 text-gray-800">{title}</span>}
      <div className="flex items-center gap-6">
        {isPolicyLocked && (
          <Tooltip text={t("policySurveysPage:unchangeablePolicy")}>
            <SvgLock fill="fill-gray-800" className="cursor-default" />
          </Tooltip>
        )}
        <InfoItem label={t("votingType")}>{referendum ? t("referendum") : t("proposal")}</InfoItem>
      </div>
      <InfoItem label={t("creationPolicy")}>{creators.length > 0 ? creators.join(", ") : "-"}</InfoItem>
      <InfoItem label={t("votingPolicy")}>{t(`approvalRequirement:${policy.approval}`)}</InfoItem>
      <InfoItem label={referendum ? t("publishers") : t("moderators")}>
        {referendum ? store.authorsIds.length : store.moderatorsIds.length}
      </InfoItem>
      <InfoItem label={t("votesRequired")}>{votesRequired >= 0 ? votesRequired : "-"}</InfoItem>
    </div>
  )
})
