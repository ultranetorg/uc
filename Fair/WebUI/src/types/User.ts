import { AuthorBase } from "./AuthorBase"
import { UserProduct } from "./UserProduct"
import { UserPublication } from "./UserPublication"
import { UserSite } from "./UserSite"

export type User = {
  id: string
  sites: UserSite[]
  authors: AuthorBase[]
  publications: UserPublication[]
  products: UserProduct[]
}
