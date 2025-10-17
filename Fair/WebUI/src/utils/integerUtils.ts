export const isInteger = (value?: string | null): boolean => {
  if (value === null || value === undefined) {
    return false
  }

  const num = Number(value)
  return Number.isInteger(num) && String(num) === value.trim()
}

export const parseInteger = (value: string): number | undefined => {
  const parsed = parseInt(value)
  return !isNaN(parsed) ? parsed : undefined
}

export function base64ToBigInt(base64: string) {
  const binary = atob(base64);
  const bytes = new Uint8Array(binary.length);
  for (let i = 0; i < binary.length; i++) {
    bytes[i] = binary.charCodeAt(i);
  }

  let result = 0n;
  for (let i = bytes.length - 1; i >= 0; i--) {
    result = (result << 8n) + BigInt(bytes[i]);
  }
  return result;
}
