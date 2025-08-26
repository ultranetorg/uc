import { useCallback, useState } from "react"
import { OptionsCollapse } from "./OptionsCollapse"
import { PropsWithClassName } from "types"
import { twMerge } from "tailwind-merge"

const TEST_ITEMS: { title: string; description: string }[] = [
  {
    title:
      "GameNest this is verrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrry looooooooooooooooooong",
    description:
      "Lorem ipsum dolor sit amet consectetur. Ut hac imperdiet laoreet lacus purus. Non facilisis euismod aenean tellus pharetra eu. Euismod non a in bibendum nullam nisi sed cursus. Tempor integer lorem sed in sem. Tortor urna eu lacus enim dictum metus volutpat lacus dictum. Netus in vitae dui in cursus aliquet euismod non auctor.",
  },
  {
    title: "PixelPioneers",
    description:
      "Eu dignissim dictum eleifend a aenean. Amet volutpat posuere est aliquam. Amet nulla amet orci ac proin blandit diam. Leo interdum volutpat integer fusce odio. Sed massa diam faucibus id. Viverra volutpat mattis arcu ante arcu convallis placerat porttitor nisi. Leo faucibus ornare cursus tellus luctus at commodo.",
  },
  { title: "QuestCraft", description: "1" },
  { title: "CodeCrafters", description: "2" },
  {
    title: "LevelUpAcademy",
    description:
      "A eget convallis sit mi turpis sem et. Arcu luctus integer rhoncus tellus leo est. Diam aliquam pellentesque fringilla ac leo facilisis ut. Non hendrerit nisl ullamcorper nec et nunc senectus. Purus dolor aliquet lectus tincidunt posuere id a morbi maecenas. Faucibus amet dignissim nam morbi porta ut urna.",
  },
  { title: "EpicVentures", description: "3" },
  {
    title: "PixelVerse",
    description:
      "Lorem ipsum dolor sit amet consectetur. Ut hac imperdiet laoreet lacus purus. Non facilisis euismod aenean tellus pharetra eu. Euismod non a in bibendum nullam nisi sed cursus. Tempor integer lorem sed in sem. Tortor urna eu lacus enim dictum metus volutpat lacus dictum. Netus in vitae dui in cursus aliquet euismod non auctor.",
  },
  {
    title: "DreamForge",
    description:
      "Eu dignissim dictum eleifend a aenean. Amet volutpat posuere est aliquam. Amet nulla amet orci ac proin blandit diam. Leo interdum volutpat integer fusce odio. Sed massa diam faucibus id. Viverra volutpat mattis arcu ante arcu convallis placerat porttitor nisi. Leo faucibus ornare cursus tellus luctus at commodo.",
  },
  {
    title: "FlyBear",
    description:
      "A eget convallis sit mi turpis sem et. Arcu luctus integer rhoncus tellus leo est. Diam aliquam pellentesque fringilla ac leo facilisis ut. Non hendrerit nisl ullamcorper nec et nunc senectus. Purus dolor aliquet lectus tincidunt posuere id a morbi maecenas. Faucibus amet dignissim nam morbi porta ut urna.",
  },
  { title: "Prussia", description: "4" },
]

type OptionsCollapsesListBaseProps = {
  items: { title: string; description: string }[]
}

export type OptionsCollapsesListProps = PropsWithClassName & OptionsCollapsesListBaseProps

export const OptionsCollapsesList = ({ className, items = TEST_ITEMS }: OptionsCollapsesListProps) => {
  const [expandedItemIndex, setExpandedItemIndex] = useState<number | undefined>()

  const handleExpand = useCallback(
    (index: number) => setExpandedItemIndex(prev => (prev !== index ? index : undefined)),
    [],
  )

  return (
    <div className={twMerge("flex flex-col gap-3", className)}>
      {items.map((x, index) => (
        <OptionsCollapse
          {...x}
          key={index}
          expanded={expandedItemIndex === index}
          onExpand={() => handleExpand(index)}
          onVoteClick={() => console.log("onVoteClick")}
        />
      ))}
    </div>
  )
}
