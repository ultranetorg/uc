export type StoresGridEmptyProps = {
  message: string
}

export const StoresGridEmpty = ({ message }: StoresGridEmptyProps) => (
  <span className="py-6 text-lg leading-5 text-gray-800">{message}</span>
)
