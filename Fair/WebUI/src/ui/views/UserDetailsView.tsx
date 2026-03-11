import { useParams } from "react-router-dom"

import { useGetModeratorUser } from "entities"

export const UserDetailsView = () => {
  const { name } = useParams()

  const { data: user } = useGetModeratorUser(name)

  return <>{JSON.stringify(user)}</>
}
