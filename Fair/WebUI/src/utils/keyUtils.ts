export function makeKey(...parts: unknown[]): string {
  return parts
    .filter(p => p !== undefined && p !== null)
    .map(p => String(p))
    .join("_")
}
