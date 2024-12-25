import "i18n"
import { QueryProvider, Router, SettingsProvider } from "app/"

import "./index.css"
import "react-tooltip/dist/react-tooltip.css"

const App = () => {
  return (
    <>
      <QueryProvider>
        <SettingsProvider>
          <Router />
        </SettingsProvider>
      </QueryProvider>
    </>
  )
}

export default App
