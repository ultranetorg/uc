import { useCallback, useMemo, useState } from "react"
import { useDebounceValue } from "usehooks-ts"
import { useTranslation } from "react-i18next"

import { SvgCheckCircle, SvgSpinner, SvgXCircleSm } from "assets"
import { SEARCH_DELAY } from "config"
import { USER_NAME_MAX_LENGTH, USER_NAME_MIN_LENGTH } from "constants/validation"
import { useGetUser } from "entities"
import { useEscapeKey } from "hooks"
import { ButtonPrimary, Input, Modal, ModalProps, ValidationWrapper } from "ui/components"
import { USER_NAME } from "utils"

import { ActiveAccount } from "./ActiveAccount"

type SignInState = "sign-in" | "sign-up"

type SignInModalBaseProps = {
  submitDisabled: boolean
  onSubmit(userName: string, address: string): void
}

export type SignInModalProps = Pick<ModalProps, "onClose"> & SignInModalBaseProps

export const SignInModal = ({ submitDisabled, onSubmit, ...rest }: SignInModalProps) => {
  const { t } = useTranslation("signInModal")

  useEscapeKey(rest.onClose)

  const [state, setState] = useState<SignInState>("sign-in")
  const [userName, setUserName] = useState("")
  const [debouncedUserName] = useDebounceValue(userName, SEARCH_DELAY)

  const title = state === "sign-in" ? t("signIn") : t("signUp")
  const footerText = state === "sign-in" ? t("dontHaveAccount") : t("alreadyHaveAccount")

  const { data: user, isFetching } = useGetUser(debouncedUserName)

  const validationMessage = useMemo(() => {
    if (
      userName &&
      ((!USER_NAME.test(userName) && userName.length < USER_NAME_MIN_LENGTH) ||
        !USER_NAME.test(userName) ||
        userName.length > USER_NAME_MAX_LENGTH)
    ) {
      return t("validation:invalidUserName")
    }

    if (user && !user.ok) {
      return t("validation:userNotFound")
    }
  }, [t, user, userName])

  const handleSubmit = useCallback(() => onSubmit(userName, user!.data!.address), [onSubmit, user, userName])

  const toggleState = () => (state === "sign-in" ? setState("sign-up") : setState("sign-in"))

  return (
    <Modal className="w-130 gap-0 p-4" {...rest}>
      <div className="flex flex-col gap-6 px-4 pb-4">
        <span className="text-center text-[44px] font-semibold first-letter:uppercase">{title}</span>
        <div className="flex flex-col gap-2">
          <span className="text-2xs font-medium first-letter:uppercase">{t("common:nickname")}</span>
          <ValidationWrapper message={validationMessage}>
            <Input
              error={!!validationMessage}
              containerClassName="h-10 px-3 py-2.5"
              placeholder={t("placeholders:enterYourNickname")}
              value={userName}
              onChange={setUserName}
              iconAfter={
                isFetching ? (
                  <SvgSpinner className="size-5 animate-spin fill-gray-300" />
                ) : state == "sign-up" && validationMessage ? (
                  <SvgXCircleSm className="stroke-error" />
                ) : state === "sign-up" && user?.ok ? (
                  <SvgCheckCircle className="size-5 stroke-light-green" />
                ) : undefined
              }
            />
          </ValidationWrapper>
        </div>
        {user?.ok && <ActiveAccount {...user.data!} onClick={handleSubmit} />}
        <div className="flex justify-end gap-6">
          <ButtonPrimary
            className="w-full px-6 capitalize"
            label={title}
            onClick={handleSubmit}
            disabled={!!validationMessage || submitDisabled || userName.length < USER_NAME_MIN_LENGTH}
          />
        </div>
        <div className="flex justify-center gap-1.5 text-2sm leading-5">
          <span>{footerText}</span>
          <span className="cursor-pointer font-medium text-gray-500 first-letter:uppercase" onClick={toggleState}>
            {state === "sign-in" ? t("signUp") : t("signIn")}
          </span>
        </div>
      </div>
    </Modal>
  )
}
