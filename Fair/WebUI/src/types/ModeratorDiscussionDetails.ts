import { ModeratorDiscussion } from "./ModeratorDiscussion"

export type ModeratorDiscussionDetails = {
  pros: string[]
  cons: string[]
  abs: string[]
} & ModeratorDiscussion
