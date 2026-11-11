import { OptionsCollapsesListSkeleton } from "ui/components/proposal"
import { ModerationHeaderSkeleton } from "ui/components/specific"

import { PolicySurveyInfoSkeleton } from "./PolicySurveyInfo.Skeleton"

const SURVEY_OPTIONS_COUNT = 4
const SURVEY_INFO_ROWS_COUNT = 5

export const PolicySurveyPageSkeleton = () => (
  <div className="flex flex-col gap-6">
    <ModerationHeaderSkeleton showSubtitle />
    <div className="flex items-start gap-8">
      <div className="flex w-full max-w-187.5 flex-col gap-8">
        <OptionsCollapsesListSkeleton count={SURVEY_OPTIONS_COUNT} expanded />
      </div>
      <PolicySurveyInfoSkeleton className="w-87.5 shrink-0" rows={SURVEY_INFO_ROWS_COUNT} />
    </div>
  </div>
)
