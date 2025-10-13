export const isUndefOrEmpty = (value?: string) => {
  return value === undefined || value === ""
}

let decoder: TextDecoder;
export function base64ToUtf8String(base64: string): string {
  const binary = atob(base64)
  const bytes = Uint8Array.from(binary, c => c.charCodeAt(0))

  if (!decoder) {
    decoder = new TextDecoder("utf-8");
  }
  return decoder.decode(bytes)
}
