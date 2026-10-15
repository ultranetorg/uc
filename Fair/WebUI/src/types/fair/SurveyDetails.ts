import { Survey } from "./Survey"
import { SurveyOptionDetails } from "./SurveyOptionDetails"

export type SurveyDetails = Omit<Survey, "options"> & {
  options: SurveyOptionDetails[]
}
