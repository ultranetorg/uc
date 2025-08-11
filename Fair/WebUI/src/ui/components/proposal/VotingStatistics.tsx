import { TFunction } from "i18next"
import { PieChart } from "react-minimal-pie-chart"
import { BaseDataEntry, Data } from "react-minimal-pie-chart/dist/commonTypes"

import { Bullet } from "ui/components"

export type VotingStatisticsProps = {
  t: TFunction
  yesCount: number
  noCount: number
  abstainedCount: number
}

export const VotingStatistics = ({ t, yesCount, noCount, abstainedCount }: VotingStatisticsProps) => {
  const total = yesCount + noCount + abstainedCount
  const yesValue = total !== 0 ? (yesCount / total) * 100 : 33
  const noValue = total !== 0 ? (noCount / total) * 100 : 33
  const absValue = 100 - yesValue - noValue

  const emptyData: Data<BaseDataEntry> = [
    {
      title: t("notVotedYet"),
      value: 100,
      color: "#9798a6",
    },
  ]
  const data: Data<BaseDataEntry> = [
    { title: t("common:yesVotes"), value: yesValue, color: "#E1A107" },
    { title: t("common:noVotes"), value: noValue, color: "#D74C41" },
    { title: t("common:abstainedVotes"), value: absValue, color: "#44B848" },
  ]

  return (
    <div className="flex w-87.5 flex-col gap-6 rounded-lg border border-gray-300 bg-gray-100 p-6 text-2sm leading-5">
      <span className="font-medium">{t("votingTitle")}</span>
      <div className="flex items-center justify-center">
        <PieChart data={total !== 0 ? data : emptyData} lineWidth={50} style={{ width: "180px", height: "180px" }} />
      </div>
      <div className="flex flex-col gap-4">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-2">
            <Bullet className="bg-light-green" />
            {t("common:yes")}
          </div>
          <div className="flex items-center gap-2">
            <span>{yesCount}</span>
            {total !== 0 && <span className="text-gray-500">{yesValue}%</span>}
          </div>
        </div>
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-2">
            <Bullet className="bg-light-red" />
            {t("common:no")}
          </div>
          <div className="flex items-center gap-2">
            <span>{noCount}</span>
            {total !== 0 && <span className="text-gray-500">{noValue}%</span>}
          </div>
        </div>
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-2">
            <Bullet className="bg-light-yellow" />
            {t("common:abstained")}
          </div>
          <div className="flex items-center gap-2">
            <span>{abstainedCount}</span>
            {total !== 0 && <span className="text-gray-500">{absValue}%</span>}
          </div>
        </div>
      </div>
    </div>
  )
}
