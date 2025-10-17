import { useCallback, useMemo, useState } from "react"
import { useTranslation } from "react-i18next"

import { SvgArrowLeft } from "assets"
import { Proposal, ProposalComment, TotalItemsResult } from "types"
import { Breadcrumbs, BreadcrumbsItemProps, ButtonOutline, ButtonPrimary } from "ui/components"
import { AlternativeOptions, CommentsSection, OptionsCollapsesList, ProposalInfo } from "ui/components/proposal"
import { useParams } from "react-router-dom"
import { ProductFields } from "../components/proposal/ProductsFields"

type PageState = "voting" | "results"

const TEST_ITEMS: { title: string; description: string; votePercents: number; voted?: boolean; votesCount: number }[] =
  [
    {
      title:
        "GameNest this is verrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrry looooooooooooooooooong",
      description:
        "Lorem ipsum dolor sit amet consectetur. Ut hac imperdiet laoreet lacus purus. Non facilisis euismod aenean tellus pharetra eu. Euismod non a in bibendum nullam nisi sed cursus. Tempor integer lorem sed in sem. Tortor urna eu lacus enim dictum metus volutpat lacus dictum. Netus in vitae dui in cursus aliquet euismod non auctor.",
      votePercents: 12,
      votesCount: 123,
    },
    {
      title: "PixelPioneers",
      description:
        "Eu dignissim dictum eleifend a aenean. Amet volutpat posuere est aliquam. Amet nulla amet orci ac proin blandit diam. Leo interdum volutpat integer fusce odio. Sed massa diam faucibus id. Viverra volutpat mattis arcu ante arcu convallis placerat porttitor nisi. Leo faucibus ornare cursus tellus luctus at commodo.",
      votePercents: 100,
      voted: true,
      votesCount: 12,
    },
    { title: "QuestCraft", description: "1", votePercents: 0, votesCount: 0 },
    { title: "CodeCrafters", description: "2", votePercents: 99, votesCount: 50 },
    {
      title: "LevelUpAcademy",
      description:
        "A eget convallis sit mi turpis sem et. Arcu luctus integer rhoncus tellus leo est. Diam aliquam pellentesque fringilla ac leo facilisis ut. Non hendrerit nisl ullamcorper nec et nunc senectus. Purus dolor aliquet lectus tincidunt posuere id a morbi maecenas. Faucibus amet dignissim nam morbi porta ut urna.",
      votePercents: 15,
      votesCount: 121,
    },
    { title: "EpicVentures", description: "3", votePercents: 50, votesCount: 50 },
    {
      title: "PixelVerse",
      description:
        "Lorem ipsum dolor sit amet consectetur. Ut hac imperdiet laoreet lacus purus. Non facilisis euismod aenean tellus pharetra eu. Euismod non a in bibendum nullam nisi sed cursus. Tempor integer lorem sed in sem. Tortor urna eu lacus enim dictum metus volutpat lacus dictum. Netus in vitae dui in cursus aliquet euismod non auctor.",
      votePercents: 50,
      votesCount: 23,
    },
    {
      title: "DreamForge",
      description:
        "Eu dignissim dictum eleifend a aenean. Amet volutpat posuere est aliquam. Amet nulla amet orci ac proin blandit diam. Leo interdum volutpat integer fusce odio. Sed massa diam faucibus id. Viverra volutpat mattis arcu ante arcu convallis placerat porttitor nisi. Leo faucibus ornare cursus tellus luctus at commodo.",
      votePercents: 90,
      votesCount: 1,
    },
    {
      title: "FlyBear",
      description:
        "A eget convallis sit mi turpis sem et. Arcu luctus integer rhoncus tellus leo est. Diam aliquam pellentesque fringilla ac leo facilisis ut. Non hendrerit nisl ullamcorper nec et nunc senectus. Purus dolor aliquet lectus tincidunt posuere id a morbi maecenas. Faucibus amet dignissim nam morbi porta ut urna.",
      votePercents: 11,
      votesCount: 0,
    },
    { title: "Prussia", description: "4", votePercents: 10, votesCount: 126 },
  ]

export type ProposalViewProps = {
  parentBreadcrumb?: BreadcrumbsItemProps
  isFetching: boolean
  proposal?: Proposal
  isCommentsFetching: boolean
  comments?: TotalItemsResult<ProposalComment>
}

export const ProposalView = ({ parentBreadcrumb, proposal, isCommentsFetching, comments }: ProposalViewProps) => {
  const { siteId } = useParams()
  const { t } = useTranslation("proposal")

  const [pageState, setPageState] = useState<PageState>("voting")

  const togglePageState = useCallback(() => setPageState(prev => (prev === "voting" ? "results" : "voting")), [])
  const productIds = useMemo(
    () =>
      proposal?.options
        ?.filter(option => option.operation.$type === "publication-creation")
        .map(option => option.operation.productId),
    [proposal],
  )

  if (!proposal || !comments) {
    return <>LOADING</>
  }

  return (
    <div className="flex flex-col gap-6">
      <div className="flex flex-col gap-2">
        <Breadcrumbs
          fullPath={true}
          items={[
            { path: `/${siteId}`, title: t("home") },
            ...(parentBreadcrumb ? [parentBreadcrumb] : []),
            { title: proposal.title || proposal.id },
          ]}
        />
        <div className="flex flex-col gap-4">
          <span className="text-3.5xl font-semibold leading-10">{proposal.title}</span>
        </div>
      </div>
      {productIds?.length ? (
        <div className="grid grid-cols-[auto_200px] gap-6">
          <div>
            <ProductFields productIds={productIds} />
          </div>
          <div>
            <ProposalInfo createdBy={proposal?.byAccount} createdAt={proposal?.creationTime} daysLeft={7} />
          </div>
        </div>
      ) : (
        ""
      )}
      <div className="flex gap-8">
        <div className="flex flex-col gap-8">
          <OptionsCollapsesList
            className="max-w-187.5"
            items={TEST_ITEMS}
            showResults={pageState == "results"}
            votesText={t("common:votes")}
          />
          <AlternativeOptions />
          <hr className="h-px border-0 bg-gray-300" />
          <CommentsSection isFetching={isCommentsFetching} comments={comments} />
        </div>
        <div className="flex flex-col gap-6">
          {pageState == "voting" ? (
            <ButtonOutline className="h-11 w-full" label="Show results" onClick={togglePageState} />
          ) : (
            <ButtonPrimary
              label="Back to options"
              onClick={togglePageState}
              iconBefore={<SvgArrowLeft className="fill-white" />}
            />
          )}
        </div>
      </div>
    </div>
  )
}
