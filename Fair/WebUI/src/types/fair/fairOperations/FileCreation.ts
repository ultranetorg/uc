import { MimeType, FileOwner } from "types"

import { BaseFairOperation } from "./BaseFairOperation"

export class FileCreation extends BaseFairOperation {
  public owner: string
  public data: string
  public mime: MimeType

  constructor(owner: FileOwner, id: string, data: string, mime: MimeType) {
    super("FileCreation")
    this.owner = `${owner}/${id}`
    this.data = data
    this.mime = mime
  }
}
