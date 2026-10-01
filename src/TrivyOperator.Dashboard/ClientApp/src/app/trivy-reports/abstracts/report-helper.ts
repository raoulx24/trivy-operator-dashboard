import { HasResources } from './trivy-report';

export interface NarrowedResourceNameInfo {
  label: string;
  buttonLink: string;
}

// TODO: i do not know yet how to name this class...
export class ReportHelper {
  static getNarrowedResourceNames(dto: HasResources): NarrowedResourceNameInfo {
    const resourceNames: string[] = dto.resources?.map((x) => x.name ?? 'unknown') ?? [];

    const label = resourceNames.slice(0, 2).join(', ');
    const buttonLink = resourceNames.length > 2 ? ` [+${resourceNames.length - 2}]` : '[...]';

    return {
      label,
      buttonLink,
    };
  }
}
