import { twMerge } from "tailwind-merge"
import { Link } from "react-router-dom"

import { SvgChevronDown, SvgFolderSoftwareXl } from "assets"
import { buildFileUrl, CategoryTreeItem, routes } from "utils"

type CategoryTreeRootItemBaseProps = {
  storeId: string
}

export type CategoryTreeRootItemProps = CategoryTreeItem & CategoryTreeRootItemBaseProps

export const CategoryTreeRootItem = ({ storeId, id, title, avatarId, active }: CategoryTreeRootItemProps) => {
  return (
    <Link
      className={twMerge(
        "flex min-w-0 cursor-pointer select-none items-center gap-1 rounded-md p-1.5 text-2xs leading-4",
        active && "bg-gray-100",
      )}
      to={routes.category(storeId, id)}
    >
      <div className="size-5 shrink-0 overflow-hidden rounded-md">
        {avatarId ? (
          <img src={buildFileUrl(avatarId)} className="size-full object-cover" />
        ) : (
          <SvgFolderSoftwareXl className="size-5 stroke-gray-800" />
        )}
      </div>
      <span className="min-w-0 flex-1 truncate" title={title}>
        {title}
      </span>
      <SvgChevronDown
        className={twMerge("shrink-0 stroke-gray-500", active && "rotate-180 transform stroke-gray-800")}
      />
    </Link>
  )
}

// import { memo } from "react"
// import { Link } from "react-router-dom"

// import { CategoryTreeItem, formatTitle, routes } from "utils"

// export type CategoryTreeProps = {
//   storeId: string
//   items: CategoryTreeItem[]
// }

// const INDENT_PX = 12

// export const CategoryTree = memo(({ storeId, items }: CategoryTreeProps) => (
//   <div className="flex flex-col gap-2 pl-2">
//     {items.map(item =>
//       item.active ? (
//         <span
//           key={item.id}
//           className="truncate text-2xs font-semibold leading-4 text-gray-800"
//           style={{ paddingLeft: item.depth * INDENT_PX }}
//           title={item.title}
//         >
//           {formatTitle(item.title)}
//         </span>
//       ) : (
//         <Link
//           key={item.id}
//           to={routes.category(storeId, item.id)}
//           className="truncate text-2xs font-medium leading-4 text-gray-500 hover:text-gray-800 hover:underline"
//           style={{ paddingLeft: item.depth * INDENT_PX }}
//           title={item.title}
//         >
//           {formatTitle(item.title)}
//         </Link>
//       ),
//     )}
//   </div>
// ))
