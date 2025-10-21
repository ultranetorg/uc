export class AutoId {
  public B: number = 0;
  public E: number = 0;

  constructor(b?: number, e?: number) {
    if (b !== undefined && e !== undefined) {
      this.B = b;
      this.E = e;
    }
  }

  toString(): string {
    return `${this.B}-${this.E}`;
  }

  // Чтение 7-bit encoded integer (как в BinaryReader.Read7BitEncodedInt())
  private read7BitEncodedInt(bytes: Uint8Array, offset: { value: number }): number {
    let count = 0;
    let shift = 0;
    let b;

    do {
      if (offset.value >= bytes.length) {
        throw new Error("Unexpected end of stream");
      }

      b = bytes[offset.value++];
      count |= (b & 0x7F) << shift;
      shift += 7;
    } while ((b & 0x80) !== 0);

    return count;
  }

  // Чтение из байтов (аналог Read method)
  read(bytes: Uint8Array): void {
    const offset = { value: 0 };
    this.B = this.read7BitEncodedInt(bytes, offset);
    this.E = this.read7BitEncodedInt(bytes, offset);
  }

  // Статический метод для конвертации Base64 в AutoId
  static fromBase64(base64String: string): AutoId {
    const autoId = new AutoId();

    // Декодируем Base64
    const binaryString = atob(base64String);
    const bytes = new Uint8Array(binaryString.length);

    for (let i = 0; i < binaryString.length; i++) {
      bytes[i] = binaryString.charCodeAt(i);
    }

    autoId.read(bytes);
    return autoId;
  }

  // Парсинг из строки формата "67465-121"
  static parse(str: string): AutoId {
    const i = str.indexOf('-');

    if (i === -1) {
      throw new Error("Invalid AutoId format");
    }

    const b = parseInt(str.substring(0, i));
    const e = parseInt(str.substring(i + 1));

    return new AutoId(b, e);
  }

  static tryParse(str: string): AutoId | null {
    try {
      return this.parse(str);
    } catch {
      return null;
    }
  }

  equals(other: AutoId): boolean {
    return this.B === other.B && this.E === other.E;
  }
}
