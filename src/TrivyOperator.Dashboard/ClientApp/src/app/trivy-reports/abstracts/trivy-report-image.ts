export interface TrivyReportImageDto<TTrivyReportImageResourceDto extends TrivyReportImageResourceDto> {
  imageDigest?: string | null;
  imageName?: string | null;
  imageRepository?: string | null;
  imageTag?: string | null;
  resourceNamespace?: string | null;
  resources?: Array<TTrivyReportImageResourceDto> | null;
}

export interface TrivyReportImageResourceDto {
  containerName?: string | null;
  kind?: string | null;
  name?: string | null;
}

export interface NarrowedResourceNameInfo {
  label: string;
  buttonLink: string;
}

// i do not know yet how to name this class...
export class ReportHelper {
  static getNarrowedResourceNames<T extends TrivyReportImageResourceDto>(
    dto: TrivyReportImageDto<T>,
  ): NarrowedResourceNameInfo {
    const resourceNames: string[] = dto.resources?.map((x) => x.name ?? 'unknown') ?? [];

    const label = resourceNames.slice(0, 2).join(', ');
    const buttonLink = resourceNames.length > 2 ? ` [+${resourceNames.length - 2}]` : '[...]';

    return {
      label,
      buttonLink,
    };
  }
}
