const { VITE_APP_API_BASE_URL: BASE_URL } = import.meta.env

/**
 * @deprecated This method is deprecated use `buildUserAvatarUrl` instead.
 */
export const buildAccountAvatarUrl = (accountId: string): string => `${BASE_URL}/accounts/${accountId}/avatar`

export const buildFileUrl = (fileId?: string): string | undefined =>
  fileId ? `${BASE_URL}/files/${fileId}` : undefined

export const buildUserAvatarUrl = (name: string): string => `${BASE_URL}/users/${name}/avatar`
