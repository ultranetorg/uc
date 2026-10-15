import { BaseFairOperation } from "./BaseFairOperation"

export class SurveyVoting extends BaseFairOperation {
  public store: string
  public survey: string
  public publisher: string
  public choice: number

  constructor(storeId: string, surveyId: string, publisherId: string, choice: number) {
    super("SurveyVoting")
    this.store = storeId
    this.survey = surveyId
    this.publisher = publisherId
    this.choice = choice
  }
}
