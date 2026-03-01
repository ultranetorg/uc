import { MimeType } from "types"

import { BaseFairOperation } from "./BaseFairOperation"

export class FileCreation extends BaseFairOperation {
  public owner: string
  public data: string
  public mime: MimeType

  constructor(owner: string, data: string, mime: MimeType) {
    super("FileCreation")
    this.owner = `4/${owner}` // 2 - author, (4 - site, owner = siteId)
    this.data = data
    this.mime = mime
  }
}
