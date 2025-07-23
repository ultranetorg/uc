export const buildSrc = (image: string | undefined, placeholderImage?: string) =>
  image ? "data:image/png;base64," + image : placeholderImage
