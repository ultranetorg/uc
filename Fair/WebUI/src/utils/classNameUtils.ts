export const getClass = (className?: string, cls?: string) => className?.split(" ").find(x => x.startsWith(`${cls}-`))
