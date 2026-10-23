export const reviewKeys = {
  root: () => ["reviews"] as const,
  all: (publicationId: string) => [...reviewKeys.root(), publicationId] as const,
}
