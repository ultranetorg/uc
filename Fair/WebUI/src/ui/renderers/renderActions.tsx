import { TFunction } from "i18next"

import { ButtonOutline, ButtonPrimary } from "ui/components"

export const renderActions = (
  t: TFunction,
  onApprove: () => void,
  onReject: () => void,
  loadingAction?: "approve" | "reject",
  locked?: boolean,
) => (
  <div className="flex justify-end gap-5">
    <ButtonPrimary
      className="h-9 w-20 capitalize"
      label={t("common:approve")}
      loading={loadingAction === "approve"}
      disabled={loadingAction === "reject" || locked}
      onClick={e => {
        onApprove()
        e.stopPropagation()
        e.preventDefault()
      }}
    />
    <ButtonOutline
      className="h-9 w-20 capitalize"
      label={t("common:reject")}
      loading={loadingAction === "reject"}
      disabled={loadingAction === "approve" || locked}
      onClick={e => {
        onReject()
        e.stopPropagation()
        e.preventDefault()
      }}
    />
  </div>
)
