const { VITE_APP_API_BASE_URL: BASE_URL } = import.meta.env

export const buildFileUrl = (fileId: string): string => `${BASE_URL}/files/${fileId}`
