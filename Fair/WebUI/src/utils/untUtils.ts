export const attosToUnt = (value?: number | bigint): number | undefined =>
  value ? Number(value) / Number(1_000_000_000_000_000_000n) : undefined

export const attosToUsd = (value?: bigint | number, rate?: number, emissionMultiplier?: number): number | undefined =>
  value !== undefined && rate !== undefined && emissionMultiplier !== undefined
    ? ((Number(value) / Number(1_000_000_000_000_000_000n)) * rate) / emissionMultiplier
    : undefined
