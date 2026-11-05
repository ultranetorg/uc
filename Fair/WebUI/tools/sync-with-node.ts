/**
 * Synchronizes constants stored on the node side (C#) with the WebUI sources:
 *
 *  - FairNode.WelcomeMessage  ->  src/ui/pages/Index_/ParadigmDescription.tsx
 *  - Operation.<ErrorName>    ->  src/utils/errorUtils.ts
 *
 * Target places are marked in the WebUI sources:
 *
 *  - `{/* @sync-start FairNode.WelcomeMessage *\/}` ... `{/* @sync-end FairNode.WelcomeMessage *\/}`
 *    (everything between the markers is regenerated as <li> items)
 *  - `// @sync Operation.<Name>` right above a `const X = "..."` declaration
 *
 * Usage:
 *   npm run sync          - write the changes
 *   npm run sync:check    - only report places that are out of sync (exit code 1 if any)
 *
 * Any problem (missing file, missing marker, unsupported C# syntax, ...) is reported to stderr
 * and the process exits with code 1, so `npm run build` is stopped and the user is notified.
 */
import { readFileSync, writeFileSync } from "node:fs"
import { dirname, relative, resolve } from "node:path"
import { fileURLToPath } from "node:url"

import { format, resolveConfig } from "prettier"

const webUiDir = resolve(dirname(fileURLToPath(import.meta.url)), "..")

const fairNodePath = resolve(webUiDir, "../cs/FairNode.cs")
const operationPath = resolve(webUiDir, "../../Net/Mcv/Operation.cs")
const paradigmDescriptionPath = resolve(webUiDir, "src/ui/pages/Index_/ParadigmDescription.tsx")
const errorUtilsPath = resolve(webUiDir, "src/utils/errorUtils.ts")

const checkOnly = process.argv.includes("--check")

type SyncResult = { path: string; content: string }

const rel = (path: string): string => relative(webUiDir, path).replaceAll("\\", "/")

const readText = (path: string): string => {
  try {
    return readFileSync(path, "utf8")
  } catch (error) {
    throw new Error(`Cannot read ${path}: ${(error as Error).message}`)
  }
}

const lineOf = (text: string, index: number): number => text.slice(0, index).split("\n").length

// ---------------------------------------------------------------------------------------------
// C# parsing
// ---------------------------------------------------------------------------------------------

const escapeRegExp = (text: string): string => text.replace(/[.*+?^${}()|[\]\\]/g, "\\$&")

/** Reads a C# regular ("...") or verbatim (@"...") string literal starting at `start`. */
const readCsString = (text: string, start: number): { value: string; end: number } => {
  const verbatim = text[start] === "@"
  let i = verbatim ? start + 2 : start + 1
  let value = ""

  for (; i < text.length; i++) {
    const ch = text[i]

    if (verbatim) {
      if (ch === '"') {
        if (text[i + 1] === '"') {
          value += '"'
          i++
          continue
        }
        return { value, end: i + 1 }
      }
      value += ch
      continue
    }

    if (ch === '"') return { value, end: i + 1 }
    if (ch === "\n") break

    if (ch !== "\\") {
      value += ch
      continue
    }

    const next = text[++i]
    const simple: Record<string, string> = { '"': '"', "'": "'", "\\": "\\", n: "\n", r: "\r", t: "\t", "0": "\0" }

    if (next in simple) {
      value += simple[next]
    } else if (next === "u" && /^[0-9a-fA-F]{4}$/.test(text.slice(i + 1, i + 5))) {
      value += String.fromCharCode(parseInt(text.slice(i + 1, i + 5), 16))
      i += 4
    } else {
      throw new Error(`unsupported escape sequence "\\${next}" at line ${lineOf(text, i)}`)
    }
  }

  throw new Error(`unterminated string literal at line ${lineOf(text, start)}`)
}

const skipTrivia = (text: string, from: number): number => {
  let i = from
  for (;;) {
    while (i < text.length && /[\s,]/.test(text[i])) i++
    if (text.startsWith("//", i)) {
      while (i < text.length && text[i] !== "\n") i++
    } else if (text.startsWith("/*", i)) {
      const end = text.indexOf("*/", i + 2)
      if (end < 0) throw new Error(`unterminated comment at line ${lineOf(text, i)}`)
      i = end + 2
    } else {
      return i
    }
  }
}

/**
 * Evaluates a C# `string[] Name = [ "a", "b", ..Other, ..Other[..^1] ];` collection expression.
 * Only string literals and spreads of other string arrays of the same file (with an optional range) are supported.
 */
const readStringArray = (source: string, name: string, visiting: string[] = []): string[] => {
  if (visiting.includes(name)) throw new Error(`circular reference: ${[...visiting, name].join(" -> ")}`)

  const declaration = new RegExp(`string\\[\\]\\s+${escapeRegExp(name)}\\s*=\\s*\\[`).exec(source)
  if (!declaration) throw new Error(`declaration of string[] "${name}" not found`)

  const items: string[] = []
  let i = declaration.index + declaration[0].length

  for (;;) {
    i = skipTrivia(source, i)

    if (i >= source.length) throw new Error(`unterminated collection "${name}"`)
    if (source[i] === "]") return items

    if (source[i] === '"' || (source[i] === "@" && source[i + 1] === '"')) {
      const literal = readCsString(source, i)
      items.push(literal.value)
      i = literal.end
      continue
    }

    if (source.startsWith("..", i)) {
      const spread = /^\.\.\s*([A-Za-z_]\w*)/.exec(source.slice(i))
      if (!spread) throw new Error(`unsupported spread expression at line ${lineOf(source, i)}`)
      i += spread[0].length

      let values = readStringArray(source, spread[1], [...visiting, name])

      const range = /^\s*\[\s*(\^?\d*)\s*\.\.\s*(\^?\d*)\s*\]/.exec(source.slice(i))
      if (range) {
        const bound = (text: string, fallback: number): number =>
          text === "" ? fallback : text.startsWith("^") ? values.length - Number(text.slice(1)) : Number(text)
        values = values.slice(bound(range[1], 0), bound(range[2], values.length))
        i += range[0].length
      }

      items.push(...values)
      continue
    }

    throw new Error(
      `unsupported syntax in "${name}" at line ${lineOf(source, i)}: "${source.slice(i, i + 20).split("\n")[0]}"`,
    )
  }
}

const readStringConst = (source: string, name: string): string => {
  const declaration = new RegExp(`const\\s+string\\s+${escapeRegExp(name)}\\s*=\\s*(@?")`).exec(source)
  if (!declaration) throw new Error(`const string "${name}" not found`)

  return readCsString(source, declaration.index + declaration[0].length - declaration[1].length).value
}

// ---------------------------------------------------------------------------------------------
// Targets
// ---------------------------------------------------------------------------------------------

const toListItem = (text: string): string => {
  const needsExpression = /[{}<>&]/.test(text) || text.trim() !== text || text === ""
  return `<li>${needsExpression ? `{${JSON.stringify(text)}}` : text}</li>`
}

const prettify = async (path: string, content: string): Promise<string> => {
  const options = (await resolveConfig(path)) ?? {}
  try {
    return await format(content, { ...options, filepath: path })
  } catch {
    // The project's prettier plugins may be unavailable; fall back to the core formatting.
    return await format(content, { ...options, plugins: [], filepath: path })
  }
}

const syncWelcomeMessage = async (): Promise<SyncResult> => {
  const messages = readStringArray(readText(fairNodePath), "WelcomeMessage")
  if (messages.length === 0) throw new Error("FairNode.WelcomeMessage is empty")

  const original = readText(paradigmDescriptionPath)
  const crlf = original.includes("\r\n")
  const text = original.replaceAll("\r\n", "\n")

  const markers =
    /^([ \t]*)\{\/\* @sync-start FairNode\.WelcomeMessage \*\/\}[ \t]*\n[\s\S]*?\n[ \t]*\{\/\* @sync-end FairNode\.WelcomeMessage \*\/\}/m
  const match = markers.exec(text)
  if (!match) {
    throw new Error(
      `markers "{/* @sync-start FairNode.WelcomeMessage */}" / "{/* @sync-end FairNode.WelcomeMessage */}" not found in ${rel(paradigmDescriptionPath)}`,
    )
  }

  const indent = match[1]
  const block = [
    `${indent}{/* @sync-start FairNode.WelcomeMessage */}`,
    ...messages.map(m => `${indent}${toListItem(m)}`),
    `${indent}{/* @sync-end FairNode.WelcomeMessage */}`,
  ].join("\n")

  const updated = await prettify(
    paradigmDescriptionPath,
    text.replace(match[0], () => block),
  )

  return { path: paradigmDescriptionPath, content: crlf ? updated.replaceAll("\n", "\r\n") : updated }
}

const syncErrors = (): SyncResult => {
  const operation = readText(operationPath)
  const original = readText(errorUtilsPath)

  const marker = /(\/\/ @sync Operation\.(\w+)[ \t]*\r?\n[ \t]*const\s+\w+\s*=\s*)"((?:[^"\\\r\n]|\\.)*)"/g
  let found = 0
  const errors: string[] = []

  const content = original.replace(marker, (whole, prefix: string, name: string) => {
    found++
    try {
      return `${prefix}${JSON.stringify(readStringConst(operation, name))}`
    } catch (error) {
      errors.push(`Operation.${name}: ${(error as Error).message}`)
      return whole
    }
  })

  if (errors.length > 0) throw new Error(errors.join("\n"))
  if (found === 0) throw new Error(`no "// @sync Operation.<Name>" markers found in ${rel(errorUtilsPath)}`)

  return { path: errorUtilsPath, content }
}

// ---------------------------------------------------------------------------------------------
// Main
// ---------------------------------------------------------------------------------------------

const tasks: { name: string; run: () => Promise<SyncResult> | SyncResult }[] = [
  { name: "FairNode.WelcomeMessage -> ParadigmDescription.tsx", run: syncWelcomeMessage },
  { name: "Operation errors -> errorUtils.ts", run: syncErrors },
]

const main = async (): Promise<void> => {
  const failures: string[] = []
  const outOfSync: string[] = []

  for (const task of tasks) {
    try {
      const { path, content } = await task.run()

      if (readText(path) === content) {
        console.log(`[sync] ${task.name}: up to date`)
      } else if (checkOnly) {
        console.log(`[sync] ${task.name}: OUT OF SYNC (${rel(path)})`)
        outOfSync.push(rel(path))
      } else {
        writeFileSync(path, content, "utf8")
        console.log(`[sync] ${task.name}: updated ${rel(path)}`)
      }
    } catch (error) {
      failures.push(`${task.name}\n    ${(error as Error).message.replaceAll("\n", "\n    ")}`)
    }
  }

  if (failures.length > 0) {
    console.error(`\n!!! SYNC FAILED !!!\n${failures.map(f => `  - ${f}`).join("\n")}\n`)
    process.exit(1)
  }

  if (outOfSync.length > 0) {
    console.error(`\nOut of sync: ${outOfSync.join(", ")}. Run "npm run sync" to fix.\n`)
    process.exit(1)
  }
}

main().catch(error => {
  console.error(`\n!!! SYNC FAILED !!!\n  ${(error as Error).stack ?? error}\n`)
  process.exit(1)
})
