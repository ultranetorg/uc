import { MutableRefObject, RefObject } from "react"

export const getVisibleItemsCount = (
  container: RefObject<HTMLDivElement>,
  items: MutableRefObject<(HTMLAnchorElement | null)[]>,
) => {
  if (!container.current) {
    return 0
  }

  const containerRight = container.current.getBoundingClientRect().right

  let count = 0
  for (let i = 0; i < items.current.length; ++i) {
    const item = items.current[i]
    if (item) {
      const itemLeft = item!.getBoundingClientRect().left
      if (containerRight >= itemLeft) {
        ++count
      }
    }
  }

  return count
}
