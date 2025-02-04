import { useDocumentTitle } from "usehooks-ts"

import { useGetUser } from "entities"

const { VITE_APP_USER_ID: USER_ID } = import.meta.env

export const UserPage = () => {
  const { isPending, data: user } = useGetUser(USER_ID)
  console.log(user)

  useDocumentTitle(user?.id ? `User - ${user?.id} | Ultranet Explorer` : "User | Ultranet Explorer")

  if (isPending || !user) {
    return <div>Loading</div>
  }

  return (
    <div className="flex flex-col gap-4 text-black">
      <span className="text-white">USER: {user.id}</span>
      <>
        {user.sites?.length > 0 ? (
          <div className="flex flex-col">
            <span>Sites</span>
            {user.sites.map(x => (
              <div key={x.id}>
                ID: {x.id} TYPE: {x.type} TITLE: {x.title} PRODUCTS COUNT: {x.productsCount} URL: {x.url}
              </div>
            ))}
          </div>
        ) : (
          <h2>🚫 NO SITES</h2>
        )}
      </>
      <>
        {user.authors?.length > 0 ? (
          <div className="flex flex-col">
            <span>Authors</span>
            {user.authors.map(x => (
              <div key={x.id}>
                ID: {x.id} TITLE: {x.title} EXPIRATION: {x.expiration} SPACE RESERVED: {x.spaceReserved} SPACE USED:{" "}
                {x.spaceUsed}
              </div>
            ))}
          </div>
        ) : (
          <h2>🚫 NO AUTHORS</h2>
        )}
      </>
      <>
        {user.publications?.length > 0 ? (
          <div className="flex flex-col">
            <span>Publications</span>
            {user.publications.map(x => (
              <div key={x.id}>ID: {x.id}</div>
            ))}
          </div>
        ) : (
          <h2>🚫 NO PUBLICATIONS</h2>
        )}
      </>
      <>
        {user.products?.length > 0 ? (
          <div className="flex flex-col">
            <span>Products</span>
            {user.products.map(x => (
              <div key={x.id}>ID: {x.id}</div>
            ))}
          </div>
        ) : (
          <h2>🚫 NO PRODUCTS</h2>
        )}
      </>
    </div>
  )
}
