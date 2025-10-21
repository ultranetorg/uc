function base64ToBytes(base64: string): Uint8Array {
  const binaryString = atob(base64);
  const bytes = new Uint8Array(binaryString.length);
  for (let i = 0; i < binaryString.length; i++) {
    bytes[i] = binaryString.charCodeAt(i);
  }
  return bytes;
}

let decoder: TextDecoder
export function base64ToUtf8String(base64: string): string {
  const binary = atob(base64)
  const bytes = Uint8Array.from(binary, c => c.charCodeAt(0))

  if (!decoder) {
    decoder = new TextDecoder("utf-8")
  }
  return decoder.decode(bytes)
}

export function base64ToBigInt(base64: string) {
  const bytes = base64ToBytes(base64)

  let result = 0n;
  for (let i = bytes.length - 1; i >= 0; i--) {
    result = (result << 8n) + BigInt(bytes[i]);
  }
  return result;
}
