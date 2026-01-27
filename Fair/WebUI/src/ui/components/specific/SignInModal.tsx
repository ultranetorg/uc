import { useCallback, useMemo, useState } from "react"
import { useDebounceValue } from "usehooks-ts"
import { useTranslation } from "react-i18next"

import { useManageUsersContext } from "app"
import { SvgCheckCircle, SvgSpinner, SvgXCircleSm } from "assets"
import { SEARCH_DELAY } from "config"
import { USER_NAME_MAX_LENGTH, USER_NAME_MIN_LENGTH } from "constants/validation"
import { useGetUser } from "entities"
import { useEscapeKey } from "hooks"
import { ButtonPrimary, Input, Modal, ModalProps, ValidationWrapper, ValidationWrapperBaseProps } from "ui/components"
import { showToast, USER_NAME_REGEXP } from "utils"

import { ActiveAccount } from "./ActiveAccount"

export type SignInModalState = "sign-in" | "sign-up"

export type SignInModalProps = Pick<ModalProps, "onClose">

export const SignInModal = (props: SignInModalProps) => {
  const { t } = useTranslation("signInModal")

  useEscapeKey(props.onClose)

  const [state, setState] = useState<SignInModalState>("sign-in")
  const [userName, setUserName] = useState("")
  const [debouncedUserName] = useDebounceValue(userName, SEARCH_DELAY)

  const { isPending, authenticate, register } = useManageUsersContext()

  const { data: user, isFetching } = useGetUser(debouncedUserName)

  const signInValidationProps = useMemo<ValidationWrapperBaseProps | undefined>(() => {
    if (userName && !USER_NAME_REGEXP.test(userName)) {
      return { message: t("validation:invalidUserName"), type: "error" }
    }

    if (user && !user.ok) {
      return { message: t("validation:userNotFound"), type: "error" }
    }
  }, [t, user, userName])

  const signUpValidationProps = useMemo<ValidationWrapperBaseProps | undefined>(() => {
    if (userName && !USER_NAME_REGEXP.test(userName)) {
      return { message: t("validation:invalidUserName"), type: "error" }
    }

    if (user) {
      if (user.ok) {
        return { message: t("validation:nicknameAlreadyInUse"), type: "error" }
      } else {
        return { message: t("validation:nicknameAvailable"), type: "success" }
      }
    }

    return { message: t("uniqueNickname"), type: "default" }
  }, [t, user, userName])

  const authenticateUser = useCallback(
    (userName: string, address: string) => {
      authenticate(userName, address!, {
        onSuccess: data => {
          if (data === null) {
            showToast(t("authenticationCancelled"), "success")
            return
          }

          showToast(t("successfullyAuthenticated", { userName }), "success")
          props.onClose?.()
        },
        onError: error => showToast(error.message, "error"),
      })
    },
    [authenticate, props, t],
  )

  const registerUser = useCallback(
    (userName: string) => {
      register(userName, {
        onSuccess: data => {
          if (data === null) {
            showToast(t("registrationCancelled"), "success")
            return
          }

          showToast(t("successfullyRegistered", { userName }), "success")
          props.onClose?.()
        },
        onError: error => showToast(error.message, "error"),
      })
    },
    [props, register, t],
  )

  const handleSubmit = useCallback(() => {
    if (state === "sign-in") authenticateUser(userName, user!.data!.address)
    else registerUser(userName)
  }, [authenticateUser, registerUser, state, user, userName])

  const toggleState = () => (state === "sign-in" ? setState("sign-up") : setState("sign-in"))

  const title = state === "sign-in" ? t("signIn") : t("signUp")

  return (
    <Modal className="w-130 gap-0 p-4" {...props}>
      <div className="flex flex-col gap-6 px-4 pb-4">
        <span className="text-center text-[44px] font-semibold first-letter:uppercase">{title}</span>
        <div className="flex flex-col gap-2">
          <span className="text-2xs font-medium first-letter:uppercase">{t("common:nickname")}</span>
          <ValidationWrapper {...(state === "sign-in" ? signInValidationProps : signUpValidationProps)}>
            <Input
              disabled={isPending}
              containerClassName="h-10 px-3 py-2.5"
              placeholder={state === "sign-in" ? t("placeholders:enterYourNickname") : t("placeholders:yourNickname")}
              value={userName}
              onChange={setUserName}
              maxLength={USER_NAME_MAX_LENGTH}
              iconAfter={
                isFetching ? (
                  <SvgSpinner className="size-5 animate-spin fill-gray-300" />
                ) : state == "sign-up" && signUpValidationProps?.type === "error" ? (
                  <SvgXCircleSm className="stroke-error" />
                ) : state === "sign-up" && signUpValidationProps?.type === "success" ? (
                  <SvgCheckCircle className="size-5 stroke-light-green" />
                ) : null
              }
            />
          </ValidationWrapper>
        </div>
        {state === "sign-in" && user?.ok && (
          <ActiveAccount disabled={isPending} {...user.data!} onClick={handleSubmit} />
        )}
        <div className="flex justify-end gap-6">
          <ButtonPrimary
            className="w-full px-6 capitalize"
            label={title}
            onClick={handleSubmit}
            disabled={
              isPending ||
              userName.length < USER_NAME_MIN_LENGTH ||
              (state === "sign-in" ? !!signInValidationProps : signUpValidationProps?.type !== "success")
            }
          />
        </div>
        <div className="flex justify-center gap-1.5 text-2sm leading-5">
          <span>{state === "sign-in" ? t("dontHaveAccount") : t("alreadyHaveAccount")}</span>
          <span className="cursor-pointer font-medium text-gray-500 first-letter:uppercase" onClick={toggleState}>
            {state === "sign-in" ? t("signUp") : t("signIn")}
          </span>
        </div>
      </div>
    </Modal>
  )
}
