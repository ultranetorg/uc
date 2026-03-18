import { BaseFairOperation } from "./BaseFairOperation"

export class ProposalCommentCreation extends BaseFairOperation {
  public proposal: string
  public author: string
  public text: string

  constructor(proposal: string, author: string, text: string) {
    super("ProposalCommentCreation")
    this.proposal = proposal
    this.author = author
    this.text = text
  }
}
