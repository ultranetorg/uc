import { Role } from "types/Role"
import { BaseVotableOperation } from "types/votableOperations"

import { BaseFairOperation } from "./BaseFairOperation"

type ProposalOption = {
  title: string
  operation: BaseVotableOperation
}

export class ProposalCreation extends BaseFairOperation {
  public site: string
  public by: string // Account Id for Moderators, Author Id for Author
  public as: Role
  public title: string
  public text: string
  public options: ProposalOption

  constructor(site: string, by: string, as: Role, title: string, text: string, options: ProposalOption) {
    super("ProposalCreation")
    this.site = site
    this.by = by
    this.as = as
    this.title = title
    this.text = text
    this.options = options
  }
}
