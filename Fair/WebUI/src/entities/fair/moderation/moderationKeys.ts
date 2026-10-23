// Keys mirror the server routes: api/moderation/stores/{storeId}/...
export const moderationKeys = {
  root: (storeId: string) => ["moderation", "stores", storeId] as const,
}
