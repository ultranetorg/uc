import { useMemo } from "react"
import { useDocumentTitle } from "usehooks-ts"

import { useSettings } from "app/"
import { SvgBarChart, SvgCoin, SvgHeartPulse, SvgSpeedometer } from "assets"
import { useGetChainData } from "entities/useGetChainData"
import { PageLoader, CardList } from "ui/components"

import { getCardListValueRenderer } from "./cardListValueRenderer"
import { costRows, performanceRows, stateRows, statisticsRows } from "./constants"

export const DashboardPage = () => {
  useDocumentTitle("Ultranet Explorer")

  const { data, isLoading } = useGetChainData()
  const { currency } = useSettings()

  const cardListValueRenderer = useMemo(
    () => getCardListValueRenderer(currency, data?.rateUsd, data?.emissionMultiplier),
    [currency, data?.rateUsd, data?.emissionMultiplier],
  )

  if (isLoading || !data) {
    return <PageLoader className="mt-10 gap-2.5" />
  }

  return (
    <div className="mt-10 flex flex-col gap-2.5">
      <div className="grid grid-cols-1 gap-2.5 lg:grid-cols-2">
        <CardList
          className="order-2 lg:order-1"
          title="State"
          icon={<SvgHeartPulse className="h-6 w-6" />}
          items={data.state}
          valueRenderer={cardListValueRenderer}
          rows={stateRows}
        />
        <CardList
          className="order-1 lg:order-2"
          title="Cost"
          icon={<SvgCoin className="h-6 w-6" />}
          items={data.cost}
          valueRenderer={cardListValueRenderer}
          rows={costRows}
        />
        <CardList
          className="order-4 lg:order-3"
          title="Performance"
          icon={<SvgSpeedometer className="h-6 w-6" />}
          items={data.performance}
          valueRenderer={cardListValueRenderer}
          rows={performanceRows}
        />
        <CardList
          className="order-3 lg:order-4"
          title="Statistics"
          icon={<SvgBarChart className="h-6 w-6" />}
          items={data.statistics}
          valueRenderer={cardListValueRenderer}
          rows={statisticsRows}
        />
      </div>
    </div>
  )
}
