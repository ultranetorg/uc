const { VITE_APP_API_BASE_URL: BASE_URL } = import.meta.env

export const buildAccountAvatarUrl = (accountId: string): string => `${BASE_URL}/accounts/${accountId}/avatar`

export const buildFileUrl = (fileId?: string): string | undefined =>
  fileId ? `${BASE_URL}/files/${fileId}` : undefined
