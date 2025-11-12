import { base64ToUtf8String } from "./base64Utils"

export function formatBase64Date(value: string) {
  const date = new Date(base64ToUtf8String(value))

  return new Intl.DateTimeFormat("en-US", {
    year: "numeric",
    month: "long",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit",
    timeZoneName: "short",
  }).format(date)
}
