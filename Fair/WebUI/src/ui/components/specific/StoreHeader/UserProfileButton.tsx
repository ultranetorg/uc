import { memo, useCallback } from "react"
import { twMerge } from "tailwind-merge"
import { Link } from "react-router-dom"
import { TFunction } from "i18next"

import { useStoreRolesContext, useUserContext } from "app"
import { useCreateProposal } from "hooks"
import { Role } from "types"
import { routes, showToast } from "utils"

import { MENU_ITEM_STYLE } from "./styles"

const ALREADY_EXISTS_ERROR = "Already exists"

export type UserProfileButtonProps = {
  storeId: string
  t: TFunction
}

export const UserProfileButton = memo(({ storeId, t }: UserProfileButtonProps) => {
  const { isJoined } = useStoreRolesContext()
  const { user } = useUserContext()

  const { execute, isExecuting } = useCreateProposal(
    // @ts-expect-error fix
    { $type: "UserRegistration" },
    { id: user?.id, role: Role.User },
    () => showToast(t("toast:userRegistrationRequested"), "success"),
    (err: Error) => {
      // Заявка на регистрацию уже была отправлена ранее — это не ошибка для пользователя.
      if (err.message === ALREADY_EXISTS_ERROR) {
        showToast(t("toast:userRegistrationAlreadyRequested"), "success")
      } else {
        showToast(t("toast:userRegistrationFailed"), "error")
      }
    },
  )

  const handleJoin = useCallback(() => execute(), [execute])

  if (!user) return null

  return !isJoined ? (
    <button
      className={twMerge(MENU_ITEM_STYLE, "w-12", isExecuting && "cursor-not-allowed opacity-50")}
      onClick={handleJoin}
      disabled={isExecuting}
    >
      {t("buttons:join")}
    </button>
  ) : (
    <Link to={routes.reviewer(storeId, user.id)} className={twMerge(MENU_ITEM_STYLE, "w-12")}>
      {t("common:profile")}
    </Link>
  )
})
