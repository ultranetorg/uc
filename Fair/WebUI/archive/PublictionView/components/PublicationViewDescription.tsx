import { useTranslation } from "react-i18next"
import { Card, TabContent, TabsList, TabsListItem } from "ui/components"
import { TabsProvider } from "app"
import { IPublicationDescription } from "../types"

export type PublicationViewDescriptionProps = {
  data: IPublicationDescription[]
  label: string
}

function getItems(lang: string, data: IPublicationDescription[]): string[] {
  return data.filter(d => d.language === lang).map(d => d.text)
}

export const PublicationViewDescription = ({ data, label }: PublicationViewDescriptionProps) => {
  const { t } = useTranslation("productPreview")

  const langs = Array.from(data.map(d => d.language))
  const defaultLang = langs.includes("en") ? "en" : langs[0]
  const tabs = data.map(
    d =>
      ({
        key: d.language,
        label: d.language,
      }) as TabsListItem,
  )

  return (
    <Card label={t(label)}>
      <TabsProvider defaultKey={defaultLang}>
        <TabsList
          className="flex gap-6"
          itemClassName="h-6 cursor-pointer text-2sm leading-4.5 text-gray-500 hover:text-gray-800"
          activeItemClassName="border-box border-b-2 border-gray-950 pb-2 text-gray-800"
          items={tabs}
        />
        <div>
          {langs.map(lang => (
            <div key={lang}>
              <TabContent when={lang}>
                {getItems(lang, data).map(text => (
                  <p key={text}>{text}</p>
                ))}
              </TabContent>
            </div>
          ))}
        </div>
      </TabsProvider>
    </Card>
  )
}
