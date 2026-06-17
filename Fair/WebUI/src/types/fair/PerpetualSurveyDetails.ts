import { PerpetualSurvey } from "./PerpetualSurvey"
import { SurveyOptionDetails } from "./SurveyOptionDetails"

export type PerpetualSurveyDetails = Omit<PerpetualSurvey, "options"> & {
  options: SurveyOptionDetails[]
}
