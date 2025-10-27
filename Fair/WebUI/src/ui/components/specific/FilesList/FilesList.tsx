import { times } from "lodash"

import { FileCard } from "./FileCard"

export const FilesList = () => times(5).map(() => <FileCard />)
