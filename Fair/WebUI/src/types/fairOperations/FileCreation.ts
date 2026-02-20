import { MimeType } from "types"

import { BaseFairOperation } from "./BaseFairOperation"

export class FileCreation extends BaseFairOperation {
  public owner: string
  public data: string
  public mime: MimeType

  constructor(owner: string, data: string, mime: MimeType) {
    super("FileCreation")
    this.owner = `2/${owner}` // 2 - author, 3 - site
    this.data = data
    this.mime = mime
  }
}
