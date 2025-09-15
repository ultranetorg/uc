export type DebugPanelProps = {
  data: object
}

export const DebugPanel = ({ data }: DebugPanelProps) => (
  <pre className="rounded bg-gray-100 p-2 font-mono text-xs">{JSON.stringify(data, null, 2)}</pre>
)
