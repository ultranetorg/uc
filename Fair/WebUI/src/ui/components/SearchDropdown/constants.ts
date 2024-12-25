export const styles = {
  controlStyle: {
    backgroundColor: "#18181b",
    borderRadius: "6px",
    height: "46px",
  },
  indicatorsContainerStyle: {
    display: "none",
  },
  inputStyle: {
    color: "#fff",
  },
  menuStyle: {
    backgroundColor: "#132C38",
    borderRadius: "6px",
    overflow: "hidden",
  },
  menuListStyle: {
    padding: "6px",

    "::-webkit-scrollbar": {
      width: "10px",
    },
    "::-webkit-scrollbar-track": {
      background: "#ffffff",
    },
    "&::-webkit-scrollbar-thumb": {
      background: "#989898",
    },
    "::-webkit-scrollbar-thumb:hover": {
      background: "#555",
    },
  },
  optionStyle: {
    alignItems: "center",
    borderRadius: "6px",
    display: "flex",
    height: "44px",
    paddingLeft: "24px",
  },
}
