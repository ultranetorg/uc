import { useTranslation } from "react-i18next"

import { ButtonOutline } from "ui/components"
import { buildSrc } from "utils"

const TEST_IMAGE =
  "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAABmZVhJZklJKgAIAAAAAQBphwQAAQAAABoAAAAAAAAAAwAAkAcABAAAADAyMzABoAMAAQAAAAEAAAAFoAQAAQAAAEQAAAAAAAAAAgABAAIABAAAAFI5OAACAAcABAAAADAxMDAAAAAAIvvHMbnUA7gAAAMPSURBVFhHxZdNTBNREMdnt0K3S79oQTRc0NAviIqJpCZEjcGLghqTwqEaA0QhhpPxrEYTu2/1wtWDgXA0mujJowm9eJOLZxJPxgsHTITtljHzdlu2+wpZ6Icv+QWYme7838y8VxbAtdIAAymAzRQANpskwLI7X81KAWy4P9QKaJPu3Dx5sk5wq6gRkQR4TsZ2CiCcuxec7SABsERDN+N2tBO43N//3W1sBhmPLYX74+OC8SC8PDR/WkFTU9HUgzhUx+/EswBK/O1RHE1NwXJBQYP58YwsxhFbWg+aTEFDD+DbybDgd3IIATIaegRLLMAxWQA/5qNCHPE+H8OypmKJKTjR1yn4nXgWMCQBlrUAGiyAf1518R2WtBCmJDE2IQFeDct4oYNPuuB34lnAu9sRNDVr94UrId5jEjQWPCbEPr3UjatTEVzNhTEb8gt+J94ESIB/X4d5SX88iWFGAt5f4utcjxBffNBr+/04eVIV/E48CTjrk3jPqez5UyomQcJtPchtBusS4tceHue+3UJXcwQs37FKTjtK27bPdyM8CVXlWrSjJr640GsL9uOtRgXQQBnMSrbFujEFErePKjJPQPb1x2QXBZDgm40K4OXXqNx+frGU9RCHbJSAD6Ou1JyG4rzVAvI1LODDdBxLGh27AJZ16/xbl4y1e2rBLlPxxom9aS/O99UIOOj2PFBAQgY03oSxpKtoMhVLegi3dZUP4A4LosGsQSR+Pts7DZUKNNyCrEq3n4JlXcUvMzHBn5Gk6q1IFxQNKO12baGX2ysCjlyBT/di/EEkYCwoC/4MyPj7RbQqYnZQ5UO7Zldgt9DADAzKgDv2WTdZiNvo4e6fs4nKXATw18swvyOcM0DH8EgVoHIO03eA/d1eSeomKVlxFG/FSXjOB3ixwyItWcd2P/YV0C4gl8225B3AKzDc2bniNrYTGAGIuo3t5L/+W159ORn4P1XYqL6Y0BoCGKkT1CpqkztXneBms+TOKSxqyWg8vjJ3fWJzMTeNi7kpG/r98H9PjpxfT/t8M+48tP4BevMu7hPw+YQAAAAASUVORK5CYII="

export type PublicationTableRowProps = {
  id: string
  title: string
  logo?: string
  publicationsCount: number
  categoryTitle?: string
  onPublicationStoresClick: (id: string) => void
}

export const PublicationTableRow = ({
  id,
  title,
  logo,
  publicationsCount,
  categoryTitle = "Software",
  onPublicationStoresClick,
}: PublicationTableRowProps) => {
  const { t } = useTranslation("profile")

  return (
    <div className="flex items-center justify-between p-4 text-2sm leading-5">
      <div className="flex w-[40%] items-center gap-2">
        <div className="h-8 w-8 overflow-hidden rounded-lg">
          <img src={buildSrc(logo, TEST_IMAGE)} className="h-full w-full object-cover" />
        </div>
        <span className="font-medium">{title}</span>
      </div>
      <span className="w-[30%]">{categoryTitle}</span>
      <div className="w-[20%]">
        <ButtonOutline
          className="h-9"
          label={`${publicationsCount} ${t("publication", { count: publicationsCount })}`}
          disabled={publicationsCount === 0}
          onClick={() => onPublicationStoresClick(id)}
        />
      </div>
    </div>
  )
}
