import { Tab, Tabs } from "ui/components"

import { DisputesTab } from "./DisputesTab"
import { PublicationsTab } from "./PublicationsTab"
import { ReviewsTab } from "./ReviewsTab"

export const ModerationPage = () => {
  return (
    <Tabs className="h-full w-full">
      <Tab id="tab-1" label="Reviews">
        <ReviewsTab />
      </Tab>
      <Tab id="tab-2" label="Publications">
        <PublicationsTab />
      </Tab>
      <Tab id="tab-3" label="Disputes">
        <DisputesTab />
      </Tab>
    </Tabs>
  )
}
