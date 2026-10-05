export const proposalCommentsKeys = {
  all: (storeId: string, proposalId: string) =>
    ["moderator", "stores", storeId, "discussions", proposalId, "comments"] as const,
}
