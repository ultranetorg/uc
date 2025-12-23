import { memo } from "react"
import { TFunction } from "i18next"

import { Controller, useForm } from "react-hook-form"
import { PROFILE_SRC } from "testConfig"
import { ButtonOutline, ButtonPrimary, Input, ValidationWrapper } from "ui/components"

import pngBackground from "./background.png"

interface IFromData {
  nickname: string
}

export type EditProfileInfoProps = {
  t: TFunction
  nickname?: string
  onDeleteAvatar: () => void
  onSubmit: (data: IFromData) => void
  onUploadAvatar: () => void
}

export const EditProfileInfo = memo(
  ({ t, nickname, onDeleteAvatar, onSubmit, onUploadAvatar }: EditProfileInfoProps) => {
    const {
      control,
      handleSubmit,
      formState: { isValid },
    } = useForm<IFromData>({
      mode: "onChange",
      defaultValues: {
        nickname: nickname || "",
      },
    })

    return (
      <form className="flex flex-col gap-6" onSubmit={handleSubmit(onSubmit)}>
        <div className="relative flex flex-col overflow-hidden rounded-lg border border-gray-300 bg-gray-100">
          <div className="bg-gray-500">
            <img src={pngBackground} alt="Background" className="size-full rounded-lg object-cover" />
          </div>
          <div className="absolute left-6 top-26.5 size-32 rounded-full bg-white p-1">
            <div className="size-30 overflow-hidden">
              <img src={PROFILE_SRC} className="size-full object-cover" />
            </div>
          </div>
          <div className="mb-2 ml-40 mt-5 flex items-center gap-4">
            <ButtonPrimary label={t("uploadAvatar")} className="h-9 w-33" onClick={onUploadAvatar} />
            <ButtonOutline label={t("delete")} className="h-9 w-25 capitalize" onClick={onDeleteAvatar} />
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
                    placeholder={t("placeholders:enterNickname") ?? nickname}
                    className="w-full"
                    value={field.value}
                    onChange={field.onChange}
                    maxLength={32}
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
          disabled={!isValid}
          loading={true}
        />
      </form>
    )
  },
)
