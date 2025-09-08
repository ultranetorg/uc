import { twMerge } from "tailwind-merge"

import { PropsWithClassName } from "types"

import { OptionsCollapse } from "./OptionsCollapse"

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

type OptionsCollapsesListBaseProps = {
  items: { title: string; description: string; votePercents: number; voted?: boolean; votesCount: number }[]
  showResults?: boolean
  votesText: string
}

export type OptionsCollapsesListProps = PropsWithClassName & OptionsCollapsesListBaseProps

export const OptionsCollapsesList = ({
  className,
  items = TEST_ITEMS,
  showResults,
  votesText,
}: OptionsCollapsesListProps) => {
  return (
    <div className={twMerge("flex flex-col gap-3", className)}>
      {items.map((x, index) => (
        <OptionsCollapse
          {...x}
          expanded={items.length === 1 ? true : undefined}
          key={index}
          showResults={showResults}
          onVoteClick={() => console.log("onVoteClick")}
          votesText={votesText}
        />
      ))}
    </div>
  )
}
