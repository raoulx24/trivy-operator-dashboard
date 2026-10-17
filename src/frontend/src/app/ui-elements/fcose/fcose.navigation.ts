import { MenuItem } from 'primeng/api';
import { BreadcrumbItemClickEvent } from 'primeng/breadcrumb';

export const DEFAULT_ROOT_NODE_ID = '00000000-0000-0000-0000-000000000000';

export interface FcoseNavigationHost {
  getNodeName: (nodeId: string) => string | undefined;
  onNavigate: (nodeId: string) => void;
}

/**
 * Breadcrumb state: the path of "dived-in" nodes starting at the root (home).
 */
export class FcoseNavigation {
  navItems: MenuItem[] = [];
  navHome?: MenuItem;
  private rootNodeId: string = DEFAULT_ROOT_NODE_ID;

  constructor(private readonly host: FcoseNavigationHost) {}

  /** Resets the breadcrumb to just the home item for the given root. */
  init(rootNodeId: string = DEFAULT_ROOT_NODE_ID) {
    this.rootNodeId = rootNodeId;
    this.navItems = [];
    this.navHome = { id: this.rootNodeId, icon: 'pi pi-sitemap' };
  }

  /** Removes everything, including the home item. */
  clear() {
    this.rootNodeId = DEFAULT_ROOT_NODE_ID;
    this.navHome = undefined;
    this.navItems = [];
  }

  onItemClick(event: BreadcrumbItemClickEvent) {
    if (event.item?.id === this.rootNodeId && this.navItems.length == 0) {
      return;
    }
    if (event.item?.id && this.navItems[this.navItems.length - 1]?.id !== event.item.id) {
      this.host.onNavigate(event.item.id);
    }
  }

  /** Updates the breadcrumb for the node that became active in the graph. */
  update(nodeId: string) {
    if (this.rootNodeId === nodeId) {
      this.navItems = [];
      return;
    }

    const potentialIndex = this.navItems.map((x) => x.id).indexOf(nodeId);
    if (potentialIndex !== -1) {
      this.navItems = this.navItems.slice(0, potentialIndex + 1);
      this.navItems[potentialIndex].class = 'breadcrumb-size';
      return;
    }

    if (this.navItems.length > 0) {
      this.navItems[this.navItems.length - 1].class = 'breadcrumb-pointer';
    }
    const name = this.host.getNodeName(nodeId);
    if (name) {
      this.navItems = [
        ...this.navItems,
        {
          id: nodeId,
          label: name,
          class: 'breadcrumb-size',
        },
      ];
    }
  }
}
