import { TFunction } from "i18next"

import { Hardware, Release, Requirements, Software } from "types"
import { SystemRequirementsTab } from "ui/components/publication"
import { SystemRequirementsTabSection } from "ui/components/publication/SystemRequirementsTabs/types"

const buildSection = (
  key: string,
  name: string,
  hardware?: Hardware,
  software?: Software,
): SystemRequirementsTabSection => {
  const values: Record<string, string> = {}

  if (hardware?.cpu) values.CPU = String(hardware.cpu)
  if (hardware?.ram) values.RAM = String(hardware?.ram)
  if (hardware?.gpu) values.GPU = String(hardware?.gpu)
  if (hardware?.hdd) values.HDD = String(hardware?.hdd)
  if (software?.os) values.OS = String(software?.os)
  if (software?.architecture) values.Architecture = String(software?.architecture)

  return {
    key,
    name,
    values,
  }
}

const buildSections = (t: TFunction, requirements: Requirements): SystemRequirementsTabSection[] => {
  const minimal = buildSection(
    `${requirements.platform.platform}-minimal`,
    t("common:minimal"),
    requirements.platform.minimal.hardware,
    requirements.platform.minimal.software,
  )

  return requirements.platform.recommended
    ? [
        minimal,
        buildSection(
          `${requirements.platform.platform}-recommended`,
          t("common:recommended"),
          requirements.platform.recommended.hardware,
          requirements.platform.recommended.software,
        ),
      ]
    : [minimal]
}

export const buildSystemRequirements = (t: TFunction, releases: Release[]): SystemRequirementsTab[] => {
  return releases.map<SystemRequirementsTab>(x => ({
    key: x.requirements.platform.platform,
    label: t(`platforms:${x.requirements.platform.platform}`),
    sections: buildSections(t, x.requirements),
  }))
}
