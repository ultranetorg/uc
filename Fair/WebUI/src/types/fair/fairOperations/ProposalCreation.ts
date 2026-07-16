import { ProposalOption, Role } from "types"

import { BaseFairOperation } from "./BaseFairOperation"

export class ProposalCreation extends BaseFairOperation {
  public store: string
  public by: string // Account Id for Moderators, Author Id for Author
  public as: Role
  public title: string
  public options: ProposalOption[]
  public text?: string

  constructor(store: string, by: string, as: Role, title: string, options: ProposalOption[], text?: string) {
    super("ProposalCreation")
    this.store = store
    this.by = by
    this.as = as
    this.title = title
    this.options = options
    this.text = text
  }
}
