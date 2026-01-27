import { SurveyOption } from "./SurveyOption"

export type PerpetualSurvey = {
  id: string
  options: SurveyOption[]
  lastWin: number
  totalVotes: number
  votesRequiredToWin: number
}
