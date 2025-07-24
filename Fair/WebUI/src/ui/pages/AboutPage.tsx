import { useSiteContext } from "app"
import { Breadcrumbs2 } from "ui/components"
import { AboutInfo } from "ui/components/specific"

export const AboutPage = () => {
  const { site } = useSiteContext()

  if (!site) {
    return <>🕑 LOADING</>
  }

  return (
    <div className="flex flex-col gap-6">
      <Breadcrumbs2 />
      <AboutInfo className="max-w-160" title={site!.title} description={site!.description!} />
    </div>
  )
}
