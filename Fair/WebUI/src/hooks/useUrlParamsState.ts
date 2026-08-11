import { useCallback, useRef, useState } from "react"
import { useSearchParams } from "react-router-dom"
import { isEqual } from "lodash"

type ParamsTypes = string | number | boolean | string[]

const ARRAY_SEPARATOR = ","

type ParamConfig<T extends ParamsTypes> = {
  defaultValue: T
  parse?: (raw: string) => T | undefined
  serialize?: (value: T) => string | undefined
  validate?: (value: T) => boolean
}

type ParamsConfig = {
  [key: string]: ParamConfig<ParamsTypes>
}

type ParamsState<C extends ParamsConfig> = {
  [K in keyof C]: C[K]["defaultValue"]
}

const defaultParse = (raw: string) => raw
const defaultSerialize = (value: ParamsTypes) => (value != null ? String(value) : undefined)
const arrayParse = (raw: string) => raw.split(ARRAY_SEPARATOR).filter(Boolean)
const arraySerialize = (value: string[]) => (value.length > 0 ? value.join(ARRAY_SEPARATOR) : undefined)

const resolveParse = (cfg: ParamConfig<ParamsTypes>) =>
  cfg.parse ?? ((Array.isArray(cfg.defaultValue) ? arrayParse : defaultParse) as NonNullable<typeof cfg.parse>)

const resolveSerialize = (cfg: ParamConfig<ParamsTypes>) =>
  cfg.serialize ??
  ((Array.isArray(cfg.defaultValue) ? arraySerialize : defaultSerialize) as NonNullable<typeof cfg.serialize>)

const parseState = <C extends ParamsConfig>(config: C, searchParams: URLSearchParams) => {
  const result = {} as ParamsState<C>

  for (const key in config) {
    const cfg = config[key]
    const raw = searchParams.get(key)
    const value = raw !== null ? resolveParse(cfg)(raw) : undefined

    result[key] = value !== undefined && (!cfg.validate || cfg.validate(value)) ? value : cfg.defaultValue
  }

  return result
}

const buildParams = <C extends ParamsConfig>(config: C, state: ParamsState<C>) => {
  const params = new URLSearchParams()

  for (const key in config) {
    const cfg = config[key]
    const serialize = resolveSerialize(cfg)
    const encoded = serialize(state[key])
    if (encoded !== undefined && encoded !== serialize(cfg.defaultValue)) params.set(key, encoded)
  }

  return params
}

export const useUrlParamsState = <C extends ParamsConfig>(config: C) => {
  const [searchParams, setSearchParams] = useSearchParams()

  // The config is written inline at the call site, so it is a fresh object on every render — pin it to keep the setter stable.
  const configRef = useRef(config)

  const [state, setState] = useState<ParamsState<C>>(() => parseState(configRef.current, searchParams))

  // Mirrors state so several updates within one tick build on each other instead of racing over a stale snapshot.
  const stateRef = useRef(state)

  // Resync when the URL changes from outside this hook (back/forward navigation, external links).
  const lastSearchParamsRef = useRef(searchParams)
  if (searchParams !== lastSearchParamsRef.current) {
    lastSearchParamsRef.current = searchParams
    const parsedState = parseState(configRef.current, searchParams)
    if (!isEqual(parsedState, stateRef.current)) {
      stateRef.current = parsedState
      setState(parsedState)
    }
  }

  const setStateInternal = useCallback(
    // Keys left out keep their current value; calling without arguments resets every param to its default.
    (newState?: Partial<ParamsState<C>>) => {
      const config = configRef.current

      let nextState: ParamsState<C>
      if (newState) {
        nextState = { ...stateRef.current, ...newState }
      } else {
        nextState = {} as ParamsState<C>
        for (const key in config) nextState[key] = config[key].defaultValue
      }

      if (isEqual(nextState, stateRef.current)) return

      stateRef.current = nextState
      setState(nextState)
      setSearchParams(buildParams(config, nextState))
    },
    [setSearchParams],
  )

  return [state, setStateInternal] as const
}
