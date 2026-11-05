// The values below are synchronized with Operation.cs by `npm run sync` (see tools/sync-with-node.ts).
// @sync Operation.Denied
const ACCESS_DENIED_MESSAGE = "Access denied"
// @sync Operation.AlreadyExists
const ALREADY_EXISTS_MESSAGE = "Already exists"

const hasMessage = (error: unknown, message: string): boolean =>
  error instanceof Error && error.message.toLowerCase().includes(message.toLowerCase())

export const isAccessDeniedError = (error: unknown): boolean => hasMessage(error, ACCESS_DENIED_MESSAGE)

export const isAlreadyExistsError = (error: unknown): boolean => hasMessage(error, ALREADY_EXISTS_MESSAGE)
