import { memo } from "react"
import { TFunction } from "i18next"
import { useNavigate } from "react-router-dom"
import { Controller, useForm } from "react-hook-form"

import { useUserContext } from "app"
import { useTransactMutationWithStatus } from "entities/mcv"
import { PROFILE_SRC } from "testConfig"
import { AccountNicknameChange } from "types"
import { ButtonPrimary, Input, ValidationWrapper } from "ui/components"
import { showToast } from "utils"

import pngBackground from "./background.png"

interface IFromData {
  nickname: string
}

export type EditProfileInfoProps = {
  t: TFunction
  nickname?: string
}

export const EditProfileInfo = memo(({ t, nickname }: EditProfileInfoProps) => {
  const {
    control,
    handleSubmit,
    formState: { isValid },
    watch,
  } = useForm<IFromData>({
    mode: "onChange",
    defaultValues: {
      nickname: nickname || "",
    },
  })
  const currentNickname = watch("nickname")

  const navigate = useNavigate()

  const { refetch } = useUserContext()
  const { mutate, isPending } = useTransactMutationWithStatus()

  const submit = (data: IFromData) => {
    const operation = new AccountNicknameChange(data.nickname)
    mutate(operation, {
      onSuccess: () => {
        showToast(t("toast:nicknameChanged"), "success")
      },
      onError: err => {
        showToast(err.toString(), "error")
      },
      onSettled: () => {
        navigate(-1)
        refetch()
      },
    })
  }

  return (
    <form className="flex flex-col gap-6" onSubmit={handleSubmit(submit)}>
      <div className="relative flex flex-col overflow-hidden rounded-lg border border-gray-300 bg-gray-100">
        <div className="bg-gray-500">
          <img src={pngBackground} alt="Background" className="size-full rounded-lg object-cover" />
        </div>
        <div className="absolute left-6 top-26.5 size-32 rounded-full bg-white p-1">
          <div className="size-30 overflow-hidden">
            <img src={PROFILE_SRC} className="size-full object-cover" />
          </div>
        </div>
        <div className="mb-2 ml-40 mt-5 flex h-8 items-center gap-4">
          {/* <ButtonPrimary label={t("uploadAvatar")} className="h-9 w-33" onClick={handleUploadAvatar} />
          <ButtonOutline label={t("delete")} className="h-9 w-25 capitalize" onClick={handleDeleteAvatar} /> */}
        </div>
        <div className="m-6 flex flex-col gap-2">
          <span className="text-2xs font-medium capitalize leading-4">{t("common:nickname")}</span>
          <Controller
            control={control}
            name="nickname"
            rules={{
              required: { value: true, message: t("validation:required") },
              minLength: { value: 4, message: t("validation:minLength", { count: 4 }) },
              maxLength: { value: 32, message: t("validation:maxLength", { count: 32 }) },
              pattern: {
                value: /^[a-z0-9]+$/,
                message: t("validation:onlyLowercaseLatinAndNumbers"),
              },
            }}
            render={({ field, fieldState }) => (
              <ValidationWrapper message={fieldState.error?.message}>
                <Input
                  autoFocus={true}
                  placeholder={t("placeholders:enterNickname") ?? nickname}
                  className="w-full"
                  value={field.value}
                  onChange={field.onChange}
                  maxLength={32}
                  disabled={isPending}
                />
              </ValidationWrapper>
            )}
          />
        </div>
      </div>
      <ButtonPrimary
        label={t("common:saveChanges")}
        className="w-33"
        type="submit"
        disabled={!isValid || currentNickname === nickname}
        loading={isPending}
      />
    </form>
  )
})
