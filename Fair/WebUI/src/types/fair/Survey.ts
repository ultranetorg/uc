import { SurveyOption } from "./SurveyOption"

export type Survey = {
  id: string
  options: SurveyOption[]
  lastWin: number
  totalVotes: number
  votesRequiredToWin: number
}
