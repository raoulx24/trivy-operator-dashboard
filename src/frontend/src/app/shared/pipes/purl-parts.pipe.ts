import { Pipe, PipeTransform } from '@angular/core';

export interface PurlParts {
  // the package type: deb, apk, npm, maven...
  type: string;
  // what the package is called; for the types where the namespace is part of the identity (npm @scope, maven group, golang
  // module path) it is included
  name: string;
  version?: string;
  // the distro qualifier (e.g. ubuntu-24.04)
  distro?: string;
}

// the types whose namespace is the vendor (the distro), not a part of the package's name: the distro says it already
const vendorNamespaceTypes = new Set(['deb', 'apk', 'rpm']);

// pkg:type/namespace/name@version?qualifiers#subpath, in parts; undefined when it is not a purl we can read (show it as it is)
@Pipe({
  name: 'purlParts',
  standalone: true,
})
export class PurlPartsPipe implements PipeTransform {
  transform(purl: string | null | undefined): PurlParts | undefined {
    const raw = purl?.trim();
    if (!raw || !raw.toLowerCase().startsWith('pkg:')) {
      return undefined;
    }

    let rest = raw.slice(4).replace(/^\/+/, '');
    const hash = rest.indexOf('#');
    if (hash >= 0) {
      rest = rest.slice(0, hash);
    }
    const question = rest.indexOf('?');
    const query = question >= 0 ? rest.slice(question + 1) : '';
    if (question >= 0) {
      rest = rest.slice(0, question);
    }

    const slash = rest.indexOf('/');
    if (slash <= 0) {
      return undefined;
    }
    const type = rest.slice(0, slash).toLowerCase();
    let path = rest.slice(slash + 1);

    let version: string | undefined;
    const at = path.lastIndexOf('@');
    if (at > 0) {
      version = decode(path.slice(at + 1));
      path = path.slice(0, at);
    }

    const segments = path.split('/').map(decode);
    const name = segments.pop();
    if (!name) {
      return undefined;
    }
    const namespace = segments.join('/');

    return {
      type,
      name: namespace && !vendorNamespaceTypes.has(type) ? `${namespace}/${name}` : name,
      version: version || undefined,
      distro: new URLSearchParams(query).get('distro') || undefined,
    };
  }
}

function decode(value: string): string {
  try {
    return decodeURIComponent(value);
  } catch {
    return value;
  }
}
