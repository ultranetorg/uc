import { useDocumentTitle } from "usehooks-ts"

export const useSiteTitle = (siteTitle?: string, pageTitle?: string) => {
  const parts = [pageTitle, siteTitle, "Fair"].filter(Boolean)
  useDocumentTitle(parts.join(" | "))
}
