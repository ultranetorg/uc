import { SvgArrowUpCircleFill } from "assets"

type ScrollToTopButtonProps = {
  visible?: boolean
}

export const ScrollToTopButton = ({ visible }: ScrollToTopButtonProps) => {
  const handleClick = () => {
    window.scrollTo({
      top: 0,
      behavior: "smooth",
    })
  }

  return (
    <>
      {visible && (
        <div className="fixed bottom-9 right-7 z-10 box-border cursor-pointer" onClick={handleClick}>
          <SvgArrowUpCircleFill className="h-12 w-12 fill-[#3dc1f2]" />
        </div>
      )}
    </>
  )
}
