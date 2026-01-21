import { useTranslation } from "react-i18next"
import { useParams } from "react-router-dom"
import { useGetPerpetualSurveyComments, useGetPerpetualSurveyDetails } from "entities/perpetualSurveys"

export const PerpetualSurveyPage = () => {
  const { t } = useTranslation()
  const { siteId, perpetualSurveyId } = useParams()

  const { data: survey } = useGetPerpetualSurveyDetails(siteId, perpetualSurveyId)
  const { data: comments } = useGetPerpetualSurveyComments(siteId, survey?.id, 0, 20)

  return (
    <>
      {JSON.stringify(survey)} {JSON.stringify(comments)}
    </>
  )
}
