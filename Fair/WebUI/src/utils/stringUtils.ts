export const kebabToCamel = (str: string): string => str.replace(/-([a-z])/g, (_, char) => char.toUpperCase())
