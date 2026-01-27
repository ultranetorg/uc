export const hasClass = (className?: string, cls?: string) => !!className?.split(" ").some(x => x.startsWith(`${cls}-`))

export const getClass = (className?: string, cls?: string) => className?.split(" ").find(x => x.startsWith(`${cls}-`))
