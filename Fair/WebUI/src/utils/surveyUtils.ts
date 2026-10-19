import { Survey } from "types"

export const isPublisherVotedInSurvey = (survey: Survey, publisherId: string): boolean =>
  survey.options.flatMap(x => x.yes).includes(publisherId)
