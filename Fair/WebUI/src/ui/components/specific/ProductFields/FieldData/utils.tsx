export function getAdded(value: unknown) {
  return <div className="text-green-700">{value as number}</div>
}

export function getRemoved(value: unknown) {
  return <div className="text-red-500 line-through opacity-75">{value as number}</div>
}
