import { useTranslation } from "react-i18next"

import { TableColumn, TableSkeleton } from "ui/components"

import { SURVEY_CATEGORIES_SIZES } from "./constants"

type PolicySurveysSkeletonProps = {
  columns: TableColumn[]
}

export const PolicySurveysSkeleton = ({ columns }: PolicySurveysSkeletonProps) => {
  const { t } = useTranslation("policySurveysPage")

  return (
    <>
      {Object.entries(SURVEY_CATEGORIES_SIZES).map(([category, count]) => (
        <div key={category} className="flex flex-col gap-3">
          <span className="text-xl font-semibold">{t(`categories.${category}`, { defaultValue: category })}</span>
          <TableSkeleton columns={columns} rows={count} />
        </div>
      ))}
    </>
  )
}
