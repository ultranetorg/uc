import { BaseFairOperation } from "./BaseFairOperation"

export class ProposalCommentEdit extends BaseFairOperation {
  public comment: string
  public text: string

  constructor(comment: string, text: string) {
    super("ProposalCommentEdit")
    this.comment = comment
    this.text = text
  }
}
