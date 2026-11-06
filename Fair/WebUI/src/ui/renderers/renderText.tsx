export const renderText = (text: string) => (
  <span className="line-clamp-2 wrap-break-word text-sm leading-4.75" title={text}>
    {text}
  </span>
)
