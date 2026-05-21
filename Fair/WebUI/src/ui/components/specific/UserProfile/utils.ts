import { TFunction } from "i18next"

export const formatRole = (t: TFunction, isPublisher: boolean, isModerator: boolean): string =>
  isPublisher && isModerator
    ? `${t("common:publisher")}, ${t("common:moderator")}`
    : isPublisher
      ? t("common:publisher")
      : isModerator
        ? t("common:moderator")
        : t("common:user")
