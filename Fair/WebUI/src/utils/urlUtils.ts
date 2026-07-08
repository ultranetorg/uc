const { VITE_APP_API_BASE_URL: BASE_URL } = import.meta.env

export const buildFileUrl = (fileId?: string): string | undefined =>
  fileId ? `${BASE_URL}/files/${fileId}` : undefined

export const buildUserAvatarByIdUrl = (userId?: string): string | undefined =>
  userId ? `${BASE_URL}/users/by-id/${userId}/avatar` : undefined

export const buildUserAvatarByNameUrl = (name?: string): string | undefined =>
  name ? `${BASE_URL}/users/by-name/${name}/avatar` : undefined

export const isValidUrl = (value: string): boolean => {
  try {
    new URL(value.startsWith("http://") || value.startsWith("https://") ? value : `https://${value}`)
    return true
  } catch {
    return false
  }
}
