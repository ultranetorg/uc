// Must match Operation.Denied on the node side.
const ACCESS_DENIED_MESSAGE = "access denied"
// Must match the "already exists" error returned by the node.
const ALREADY_EXISTS_MESSAGE = "already exists"

const hasMessage = (error: unknown, message: string): boolean =>
  error instanceof Error && error.message.toLowerCase().includes(message)

export const isAccessDeniedError = (error: unknown): boolean => hasMessage(error, ACCESS_DENIED_MESSAGE)

export const isAlreadyExistsError = (error: unknown): boolean => hasMessage(error, ALREADY_EXISTS_MESSAGE)
