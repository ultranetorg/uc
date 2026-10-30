export const renderTitle = (title: string, text: string) => (
  <div className="flex flex-col gap-1" title={title}>
    <span className="truncate text-2sm leading-5">{title}</span>
    <span className="truncate text-2xs leading-4 text-gray-500">{text}</span>
  </div>
)
