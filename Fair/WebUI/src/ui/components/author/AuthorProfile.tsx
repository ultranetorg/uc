import { memo } from "react"
import { twMerge } from "tailwind-merge"

import { ExpandableText } from "ui/components"
import { AuthorLink, PropsWithClassName } from "types"

import { ProfileLinks } from "./ProfileLinks"

const TEST_AUTHOR_IMAGE =
  "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAHgAAAB4CAYAAAA5ZDbSAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAABmZVhJZklJKgAIAAAAAQBphwQAAQAAABoAAAAAAAAAAwAAkAcABAAAADAyMzABoAMAAQAAAAEAAAAFoAQAAQAAAEQAAAAAAAAAAgABAAIABAAAAFI5OAACAAcABAAAADAxMDAAAAAAIvvHMbnUA7gAAAjjSURBVHhe7Z1raxxVGMfzEfwI/Qh+hH1h0kk2uzvdTdIk9LJtbYx4yUq9VEWN2iBe6FawFEXdilYQwQahb0RIwRcVfVFQVBS0AUXEC/FairdHzq7TJP9ndnNmZ85ldp8f/N5szjNzZp7MzJkzZ+aMjAwAVCjcQKOjdRobO0/FItHu3f07NnaZRkdP0U03FXA9giVobCxsJwKTY8ogoHbSC4UbsC5CBrSP0CBosR3vzitUq4VYTyEh7aOG71zfvCKn8wRQGBZo9+6NmB2ZB1u4PcL/0MREPWaH5dUruH1Dy4AlFh3eRNPkpDoV4w4ZTMvly7j9A02Or7FpbeC+GCioWGzGbPRwGQSE+2UgoHKZb+wwGwSP4D7KJTQ11WAbJ3YslTZwf+UKKhbtdSfm2SDYhfvOe+SUnNAwzMcpu91vjJUX9Zya8vt2iur1XazSYjInJvzsHKGjR29klRX7s1Lxq/ElyTXg5KQfSVYtQFY5MRvLZbdJpuPHJbmmLRbdXZNZZUQzlkpruO+N0x6nhBURzXnggL375PYwFayAaF4bPV5UKskTIYdiPjKFwlAaVa6dmDDXsmYrE904M9PE3KSGxsfluuuTrVZ2g/ClM8NPMU99gwsWPbFaTT/Gi0qls2zBojdivhKDCxS9s/9eLvVsMmaBomdi3rTBBYmeWqkkfyBBQSBHb47E/O0ILkD03JkZ/aOYxsfX2AJE78U8dgUDrfrMM0Svv+7e+Xlet8i5Of6bD05OnsJcMigI3L3OqT6g4gO//srrFjk93SmDv3si5pOBAVb9+Wfc1W4olXjdIv/6q1Pmgw/433wwCHp/ToIF2PLuu3E3u+GNN3jdIl97bXvZQ4d4Gdf2epxIY2PuuiV94N9/eb0iu10+sJwHYl6vgwWtefky7jY3zM7yukX+8guW7vDee7ysa7udpllBG0aNFtd8+imvW+QDD2Dp7fRqcbswbtA8haGbb1L9+SfuLjdgvbaqA8Y4FvOrOjdYIeOePk109ap777+f1y3y888xlfG88w6PdWm9vn3EBysgdjo0klCr8WW4slze7PSg8+flnd44Vas6KbgMh24muFiU72igb76JqdNjdZUvy5GbCS6X5VsaW1Wfn0hDr54wi24mOOaPQ+1vv2HKktGrw8Smo6M3SoLRlRVMV3+oJ1K4bNtGT5fYH4bZLHFx67nVWu3KCDWbMqg9cn0dUxTP11/jL/H8/Tdfh2VVB0eIP2bunj246XZRicM6ofv3Y1R3VPlXXsFf43n5Zb4ui6ouymX8MXNVb5FLsD5x6t7znjixGfPPP/jXeBy+LK++JWl2/NULL+Dm2uWhh3id0Lffxqh4cKRHt0eIyLVrfJ2WHKFazdw9sPrPdYl6xId1QpNcPiYnebxuh8hzz/FYC6oEr+OPmbmxgZtpF51Jsn7/HaPiOXeOx0bqnt4xzoLmPhp6zz24eXbRadw0mxgVjxqHhbFbrVQwIh48xVvQ3NdyXKJ7zdNFDUrAWPTCBYyK56mneKxB1YMG9mNqv/gCN8suOo/tvv0Wo+J5/30e201dMM6gqhXNfkxtvd5d3REcqj8YY3U8eJDXBz18GNfWHYyNVP8g6I8/YnQ8P/zAl2fIEdq711wjCz12DDe1O6baBkpdFhZ4rHKnMVo6PPYYX64BVUeH2fvgrepisqP+3XdxbfF89x2PjcwKXK4BVSPLzljozz7DzYtHncIxNiurVVxbdzA2Ul2Ts+Kbb/jyM3aE5ubMj+ZIMrbJRJsgUrfL9ORJHqtM8g+iy4MP8vVkqJ0p5nQ7Ai5e5LFZeeYMri0e1fGBsZF//IGlswHXk6Hmnwe/9RZuTncwNiuTdJmqTguMVz75JJbMjq++4uvLwnLZcILDEDelOzq3N/2qbkt0UP3KGBtpGtXzh+tM6549F80mWLef96efiB5+2IzPPotri6fXOKpXX+00rvpVF1xvWhcWOh9KM9JdqbtjfWHvXr4NWan7aqzqAcTYNK6tdd5uoDBcZX9Ma574+GNe/6z98ktcazx33cVj+7Sd3HaCp6aybUl//z1W22+w/qbUBeP69HqC20mOKdCXi4tYXb9ZWuLbYMr77sO1x/PJJzw2qTMz26fJYwX6NU+ohwNYf9Pqjty8/XYem8RaLdye4EolfZ/0hx9iNf3GRONSR10wLoHbkttO8OxsuuuwL2/q66J6tnAbbKm6J3X46CMeqynmtw0WSmT0eaE8oPqksf62VQ8adFBtGozdyVKphblto6YcZ4XF3Il5vY76OgsWFvMn5nUbWFjMmbOz8afnCHX/xILE3Ij5ZFCzKd/ryKvT0zsnWMECxXy4stJ5o38npLGVTzGPPcFg0XMPH65jDntCjz5q/r1hMTMxf1rgQkRPXVxMdvRGWBlxKaZzfLy/ozeCJib4QkV/DMN0077LfbHHBoH+XEm9oH37pHfLQzFPqcCFi46dn1/GHKWCjh+XBpcv1mrZHr0RdPCgnKo9EPOSKc7GMYkdn38+/bTuvSAiaVW7cmkpm1bzTlCjIddj21arZk/NCDUabqbhGUaDwG5yI+i222SWcBs2GtunyLGJmnmaVUjMzjNn0nVFZgHNz8twWxOeO6c3QsMGkuSMbbX8SW4ETU/L6ToLL1zwL7kR6l6NVVjUt9Vyf83dCbrzTrmFSmql4uZWqF/oxRfNT/YxKNZqfL7fPNDu1pQRIb1dXNycJTSv0L33ynUZVQ9t8nC91YWCQCbgirzlFjsPDVxAx44N79Gsprq7dKmA+2TgoLW1Xe3pbHAHDLJLS71f6xxE6PTpOtsRg+aRI4N7OtaF7rhj8BJ96NAGtVrungL5SPuI9mT27L7dt6/zhVehO3Tp0i7V0mQ7z1fVBCIrK9kOYx0W6PHHQ5qZ8e9JlbqPrdfPymk4Q6jZLKiPXLOdbUv1yeGFBTlSbUHLywU6cmRVa+q5pKojtFpdp5tvbtDyshylPkFPP12gEycadOutZ6lavUhzc+t09GhnZrQDB1QLt5O8/fvXqV5fbQ8gfOmlOj3xxOB0Hf7Pf8sfsCvx2/11AAAAAElFTkSuQmCC"

const LABEL_CLASSNAME = "text-2xs font-medium leading-4 text-gray-500"

type AuthorProfileBaseProps = {
  title: string
  nickname: string
  description: string
  links?: AuthorLink[]
  registeredDate: number
  aboutLabel: string
  authorLabel: string
  linksLabel: string
  readLessLabel: string
  readMoreLabel: string
}

export type AuthorProfileProps = PropsWithClassName & AuthorProfileBaseProps

export const AuthorProfile = memo(
  ({
    className,
    title,
    nickname,
    description,
    links,
    registeredDate,
    aboutLabel,
    authorLabel,
    linksLabel,
    readLessLabel,
    readMoreLabel,
  }: AuthorProfileProps) => (
    <div className={twMerge("flex items-start gap-6 rounded-lg border border-gray-300 bg-gray-100 p-6", className)}>
      <div className="max-h-30 min-h-30 min-w-30 max-w-30 overflow-hidden rounded-lg" title={title}>
        <img src={TEST_AUTHOR_IMAGE} className="h-full w-full object-cover" />
      </div>
      <div className="flex flex-col gap-6">
        <div className="flex flex-col gap-1">
          <span className="text-xl font-semibold leading-6">{title}</span>
          <span className="text-2xs leading-4 text-gray-500">{nickname}</span>
        </div>
        <div className="flex flex-col gap-2">
          <span className={LABEL_CLASSNAME}>{aboutLabel}</span>
          <ExpandableText
            className="text-2sm leading-5"
            text={description}
            readLessLabel={readLessLabel}
            readMoreLabel={readMoreLabel}
          />
        </div>
        {links && links.length > 0 && (
          <div className="flex flex-col gap-2">
            <span className={LABEL_CLASSNAME}>{linksLabel}</span>
            <ProfileLinks links={links} />
          </div>
        )}
        <div className="flex items-center gap-4 text-2sm leading-5">
          <span>{authorLabel}</span>
          <span className="leading-3.5 text-gray-500">|</span>
          <span>{registeredDate}</span>
        </div>
      </div>
    </div>
  ),
)
