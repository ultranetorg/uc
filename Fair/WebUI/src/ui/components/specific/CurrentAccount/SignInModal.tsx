import { useCallback, useMemo, useState } from "react"
import { useDebounceValue } from "usehooks-ts"
import { useTranslation } from "react-i18next"

import { SvgCheckCircle, SvgSpinner, SvgXCircleSm } from "assets"
import { SEARCH_DELAY } from "config"
import { useGetUser } from "entities"
import { useEscapeKey } from "hooks"
import { ButtonPrimary, Input, ValidationWrapper } from "ui/components"
import { Modal, ModalProps } from "ui/components/Modal"
import { USER_NAME } from "utils"
import { USER_NAME_MAX_LENGTH, USER_NAME_MIN_LENGTH } from "constants/validation"

import { ActiveAccount } from "./ActiveAccount"

type SignInModalBaseProps = {
  submitDisabled: boolean
  onSubmit(userName: string, address: string): void
  submitLabel: string
}

export type SignInModalProps = Pick<ModalProps, "title" | "onClose"> & SignInModalBaseProps

export const SignInModal = ({ submitDisabled, onSubmit, submitLabel, title, ...rest }: SignInModalProps) => {
  const { t } = useTranslation()

  useEscapeKey(rest.onClose)

  const [userName, setUserName] = useState("")
  const [debouncedUserName] = useDebounceValue(userName, SEARCH_DELAY)

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

  return (
    <Modal className={"p-4"} {...rest}>
      <div className="flex flex-col gap-6 px-12 pb-12 pt-6">
        <span className="text-center text-[3rem] font-semibold first-letter:uppercase">{title}</span>
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
                ) : validationMessage ? (
                  <SvgXCircleSm className="stroke-error" />
                ) : user?.ok ? (
                  <SvgCheckCircle className="size-5 stroke-light-green" />
                ) : undefined
              }
            />
          </ValidationWrapper>
        </div>
        {user?.ok && <ActiveAccount {...user.data!} onClick={handleSubmit} />}
        <div className="flex justify-end gap-6">
          <ButtonPrimary
            className="w-97.5 px-6 capitalize"
            label={submitLabel}
            onClick={handleSubmit}
            disabled={!!validationMessage || submitDisabled}
          />
        </div>
      </div>
    </Modal>
  )
}
