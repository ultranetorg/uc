import { useCallback, useState } from "react"
import { useSearchParams } from "react-router-dom"
import { isEqual } from "lodash"

type ParamsTypes = string | number | boolean

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

export const useUrlParamsState = <C extends ParamsConfig>(config: C) => {
  const [searchParams, setSearchParams] = useSearchParams()

  const initialState = () => {
    const result = {} as ParamsState<C>

    for (const key in config) {
      const { defaultValue, parse = v => v, validate } = config[key]

      const raw = searchParams.get(key)
      if (raw !== null) {
        const value = parse(raw)
        if (value !== undefined && validate && validate(value)) {
          result[key] = value
          continue
        }
      }

      result[key] = defaultValue
    }

    return result
  }

  const [state, setState] = useState<ParamsState<C>>(initialState)

  const updateSearchParams = useCallback(
    (newState: ParamsState<C>) => {
      const nextParams = new URLSearchParams()

      for (const key in newState) {
        const { defaultValue, serialize = v => (v != null ? String(v) : null) } = config[key]

        const value = newState[key]

        const encoded = serialize(value)
        const defaultEncoded = serialize(defaultValue)

        if (encoded !== defaultEncoded && encoded != undefined) {
          nextParams.set(key, encoded)
        } else {
          nextParams.delete(key)
        }

        setSearchParams(nextParams)
      }
    },
    [config, setSearchParams],
  )

  const getEmptyState = useCallback(() => {
    const result = {} as ParamsState<C>

    for (const key in config) {
      const { defaultValue } = config[key]
      result[key] = defaultValue
    }

    return result
  }, [config])

  const setStateInternal = useCallback(
    (newState: ParamsState<C> | undefined = undefined) => {
      const stateToSet = newState ?? getEmptyState()
      if (!isEqual(stateToSet, state)) {
        setState(stateToSet)
        updateSearchParams(stateToSet)
      }
    },
    [getEmptyState, state, updateSearchParams],
  )

  return [state, setStateInternal] as const
}
