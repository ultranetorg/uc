import { memo, useCallback } from "react"
import { twMerge } from "tailwind-merge"
import { Link, useParams } from "react-router-dom"
import { TFunction } from "i18next"

import { useOperationPolicy, useSiteRolesContext, useUserContext } from "app"
import { useCreateProposal } from "hooks/useCreateProposal"
import { showToast } from "utils"

import { MENU_ITEM_STYLE } from "./styles"

export type UserProfileButtonProps = {
  t: TFunction
}

export const UserProfileButton = memo(({ t }: UserProfileButtonProps) => {
  const { creator } = useOperationPolicy("user-registration")
  const { isJoined } = useSiteRolesContext()
  const { siteId } = useParams()
  const { user } = useUserContext()

  const { execute, isExecuting } = useCreateProposal(
    // @ts-expect-error fix
    { $type: "UserRegistration" },
    creator,
    () => showToast(t("toast:userRegistrationRequested"), "success"),
    () => showToast(t("toast:userRegistrationFailed"), "error"),
  )

  const handleJoin = useCallback(() => execute(), [execute])

  if (!user) return null

  return !isJoined ? (
    <button
      className={twMerge(MENU_ITEM_STYLE, "w-12", isExecuting && "cursor-not-allowed opacity-50")}
      onClick={handleJoin}
      disabled={isExecuting}
    >
      {t("common:join")}
    </button>
  ) : (
    <Link to={`/${siteId}/u/${user.id}`} className={twMerge(MENU_ITEM_STYLE, "w-12")}>
      {t("common:profile")}
    </Link>
  )
})
