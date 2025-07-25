import { useParams } from "react-router-dom"
import { useTranslation } from "react-i18next"

import { useGetAuthorReferendum } from "entities"
import { ProposalInfo, Voting, VotingStatistics } from "ui/components/proposal"
import { GovernanceModerationHeader } from "ui/components/specific"

export const ReferendumPage = () => {
  const { siteId, referendumId } = useParams()
  const { t } = useTranslation("referendum")

  const { isPending, data: referendum } = useGetAuthorReferendum(siteId, referendumId)

  if (isPending || !referendum) {
    return "Loading..."
  }

  return (
    <div className="flex flex-col gap-6">
      <GovernanceModerationHeader
        siteId={siteId!}
        title={t("title", { referendumId })}
        parentBreadcrumb={{ path: `/${siteId}/a-r`, title: t("referendums:title") }}
        homeLabel={t("common:home")}
      />
      <div className="flex gap-8">
        <ProposalInfo text={referendum.text} />
        <div className="flex flex-col gap-6">
          <VotingStatistics
            t={t}
            yesCount={referendum.yesCount}
            noCount={referendum.noCount}
            absCount={referendum.absCount}
          />
          <Voting
            t={t}
            siteId={siteId!}
            daysLeft={referendum.expiration}
            createdAt={referendum.expiration}
            createdById={referendum.byId}
            createdByTitle={referendum.byNickname ?? referendum.byAddress}
            createdByAvatar={referendum.byAvatar}
          />
        </div>
      </div>
    </div>
  )
}

/*
      <div>
        <div>ID</div>
        <div>{referendum.id}</div>
      </div>
      <div>
        <div>Text</div>
        <div>{referendum.text}</div>
      </div>
      <div>
        <div>Expiration</div>
        <div>{referendum.expiration}</div>
      </div>
      <div>
        <div>Votes</div>
        <div>
          <span className="text-red-500">{referendum.yesCount}</span> /{" "}
          <span className="text-green-500">{referendum.noCount}</span> /{" "}
          <span className="text-gray-500">{referendum.absCount}</span>
        </div>
      </div>
      <div>
        <div>Pros</div>
        <div>{referendum.pros.join(",")}</div>
      </div>
      <div>
        <div>Cons</div>
        <div>{referendum.cons.join(",")}</div>
      </div>
      <div>
        <div>Abs</div>
        <div>{referendum.abs.join(",")}</div>
      </div>
      <div>
        <div>Type:</div>
        <div>{t(referendum.option.$type, { ns: "votableOperations" })}</div>
      </div>
      <div>
        <div>Type:</div>
        <div>{JSON.stringify(referendum.option)}</div>
      </div>
*/
