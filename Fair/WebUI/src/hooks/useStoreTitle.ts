import { useDocumentTitle } from "usehooks-ts"

export const useStoreTitle = (storeTitle?: string, pageTitle?: string) => {
  const parts = [pageTitle, storeTitle, "Fair"].filter(Boolean)
  useDocumentTitle(parts.join(" | "))
}
