import { TFunction } from "i18next"

import { ListRow, TableColumn } from "ui/components"

export const getListRows = (t: TFunction<string, undefined, string>): ListRow[] => [
  { accessor: "id", label: t("id") },
  { accessor: "parentId", label: t("parentId") },
  { accessor: "hash", label: t("hash") },
  { accessor: "consensusTime", label: t("consensusTime") },
  { accessor: "consensusExeunitFee", label: t("consensusExeunitFee"), type: "currency" },
  { accessor: "rentPerByte", label: t("rentPerByte"), type: "currency" },
  { accessor: "emission", label: t("emission"), type: "currency" },
  { accessor: "members", label: t("members") },
  { accessor: "funds", label: t("funds") },
  { accessor: "emissions", label: t("emissions") },
  { accessor: "domainBids", label: t("domainBids") },
  { accessor: "feePerExecutionUnit", label: t("feePerExecutionUnit"), type: "currency" },
  { accessor: "membersCount", label: t("membersCount") },
  { accessor: "consensusMemberLeaversCount", label: t("consensusMemberLeaversCount") },
  { accessor: "consensusFundJoinersCount", label: t("consensusFundJoinersCount") },
  { accessor: "consensusFundLeaversCount", label: t("consensusFundLeaversCount") },
  { accessor: "consensusViolatorsCount", label: t("consensusViolatorsCount") },
]

export const getTableColumns = (
  t: TFunction<string, undefined, string>,
  isMediumView: boolean,
  isSmallView: boolean,
): TableColumn[] => {
  if (isSmallView) {
    return [{ accessor: "id", label: t("id"), className: "w-full text-right" }]
  }
  if (isMediumView) {
    return [
      { accessor: "id", label: t("id"), className: "w-4/12 text-right" },
      { accessor: "signer", label: t("signer"), className: "w-8/12 text-left" },
    ]
  }

  return [
    { accessor: "id", label: t("id"), className: "w-2/12 text-right" },
    { accessor: "operationsCount", label: t("operationsCount"), className: "w-3/12 text-right" },
    { accessor: "signer", label: t("signer"), className: "w-7/12 text-left" },
  ]
}
