export type ChangedPublication = {
  id: string
  type: string
  title?: string
  logoId?: string
  updated: number
  authorId: string
  authorTitle: string
  authorLogoId?: string
  categoryId: string
  categoryTitle: string
  currentVersion: number
  latestVersion: number
}
