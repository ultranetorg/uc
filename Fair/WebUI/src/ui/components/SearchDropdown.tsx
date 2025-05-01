import { ChangeEvent, KeyboardEvent, memo } from "react"

import { twMerge } from "tailwind-merge"

export type SearchDropdownItem = {
  id: string
  value: string
}

export type SearchDropdownProps = {
  className?: string
  isDropdownHidden?: boolean
  items?: SearchDropdownItem[]
  value: string
  onChange: (value: string) => void
  onKeyDown?: (key: string) => void
  onSelectItem: (item: SearchDropdownItem) => void
}

export const SearchDropdown = memo(
  ({ className, isDropdownHidden, items, value, onChange, onKeyDown, onSelectItem }: SearchDropdownProps) => {
    const handleKeyDown = (e: KeyboardEvent<HTMLInputElement>) => onKeyDown && onKeyDown(e.key)
    const handleChange = (e: ChangeEvent<HTMLInputElement>) => onChange && onChange(e.target.value)

    return (
      <div style={{ position: "relative", width: "200px" }} className={twMerge("h-full border", className)}>
        <input
          type="text"
          value={value}
          style={{ width: "100%", padding: "8px", height: "100%" }}
          placeholder="Search..."
          onChange={handleChange}
          onKeyDown={handleKeyDown}
        />
        {isDropdownHidden !== true && items && items.length > 0 && (
          <ul
            style={{
              position: "absolute",
              top: "100%",
              left: 0,
              right: 0,
              background: "white",
              border: "1px solid #ccc",
              listStyle: "none",
              margin: 0,
              padding: 0,
              maxHeight: "150px",
              overflowY: "auto",
              zIndex: 1,
            }}
          >
            {items.map(item => (
              <li
                key={item.id}
                onClick={() => onSelectItem && onSelectItem(item)}
                style={{ padding: "8px", cursor: "pointer" }}
                onMouseDown={e => e.preventDefault()} // чтобы клик срабатывал до blur
              >
                {item.value}
              </li>
            ))}
          </ul>
        )}
      </div>
    )
  },
)
