import { Svg4, SvgConnectivity, SvgUltranet } from "assets"
import { ReactNode, memo } from "react"
import { useNavigate } from "react-router-dom"

import { PropsWithClassName } from "types"
import { Button } from "ui/components"

type ErrorMessageBaseProps = {
  image?: ReactNode
  title: string
  message?: ReactNode
}

type ErrorMessageProps = PropsWithClassName<ErrorMessageBaseProps>

export const ErrorMessage = memo((props: ErrorMessageProps) => {
  const { image, title, message } = props

  const navigate = useNavigate()

  const handleClick = () => navigate("/", { replace: true })

  return (
    <div className="flex w-96 flex-col items-center gap-6">
      <div className="flex select-none flex-col gap-2">
        <div className="flex flex-col items-center gap-4">
          {image}
          <div className="text-center font-sans-medium text-2xl">{title}</div>
        </div>
        {message && <div className="text-center">{message}</div>}
      </div>
      <Button label="Go to Homepage" onClick={handleClick} />
    </div>
  )
})

export const NotFoundError = () => {
  return (
    <ErrorMessage
      image={
        <div className="flex gap-2">
          <Svg4 />
          <SvgUltranet className="h-10 w-10" />
          <Svg4 />
        </div>
      }
      title="Page Not Found"
      message={
        <>
          <p>Sorry, we can’t find the page you were looking for.</p>
          <p>
            The address may have changed but the resource may still be here, check it by going through the menu at the
            top.
          </p>
        </>
      }
    />
  )
}

export const UnknownError = () => {
  return (
    <ErrorMessage
      image={<SvgConnectivity />}
      title="Ooooops...... something went wrong"
      message={<p>Try to refresh this page or feel free to contact us if the problem persist.</p>}
    />
  )
}
