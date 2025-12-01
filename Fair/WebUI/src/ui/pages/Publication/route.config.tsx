import { RouteObject } from "react-router-dom"
import { PublicationPage } from "./PublicationPage"
import { PublicationMoviePage } from "./pages/PublicationMoviePage"
import { PublicationMusicPage } from "./pages/PublicationMusicPage"
import { PublicationBookPage } from "./pages/PublicationBookPage"
import { PublicationGamePage } from "./pages/PublicationGamePage"
import { PublicationDefaultPage } from "./pages/PublicationDefaultPage"

export const PublicationRoute: RouteObject = {
  path: "publications/:publicationId",
  element: <PublicationPage />,
  children: [
    {
      path: "",
      children: [
        { path: "software", element: <PublicationMoviePage /> },
        { path: "movie", element: <PublicationMoviePage /> },
        { path: "music", element: <PublicationMusicPage /> },
        { path: "book", element: <PublicationBookPage /> },
        { path: "game", element: <PublicationGamePage /> },
        { index: true, element: <PublicationDefaultPage /> },
      ],
    },
  ],
}
