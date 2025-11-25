import { useRef, useLayoutEffect } from "react"

export function useInfiniteScrollWithPosition(
  fetchNextPage: () => void,
  canLoad: boolean,
  itemsLength: number,
  isFetching: boolean,
) {
  const scrollRef = useRef<HTMLDivElement>(null)
  const loaderRef = useRef<HTMLDivElement>(null)
  const scrollPosRef = useRef(0)

  useLayoutEffect(() => {
    if (scrollRef.current && scrollPosRef.current > 0 && !isFetching) {
      scrollRef.current.scrollTop = scrollPosRef.current
      scrollPosRef.current = 0
    }
  }, [itemsLength, isFetching])

  useLayoutEffect(() => {
    if (!canLoad) return

    const observer = new IntersectionObserver(entries => {
      if (entries[0].isIntersecting && scrollRef.current) {
        scrollPosRef.current = scrollRef.current.scrollTop
        fetchNextPage()
      }
    })

    const el = loaderRef.current
    if (el) observer.observe(el)

    return () => {
      if (el) observer.unobserve(el)
    }
  }, [canLoad, fetchNextPage])

  return { scrollRef, loaderRef }
}
