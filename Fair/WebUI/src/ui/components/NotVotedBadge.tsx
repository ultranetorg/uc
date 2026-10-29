import { memo } from "react"
import { useTranslation } from "react-i18next"
import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

export const NotVotedBadge = memo(({ className }: PropsWithClassName) => {
  const { t } = useTranslation("common")

  return <span className={twMerge("w-fit rounded bg-warning px-2 py-0.5 text-2xs", className)}>{t("notVoted")}</span>
})
