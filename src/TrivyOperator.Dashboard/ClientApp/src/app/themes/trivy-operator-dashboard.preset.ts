import { definePreset } from '@primeng/themes';
import Nora from '@primeng/themes/nora';

export const trivyOperatorDashboardPreset = definePreset(Nora, {
  semantic: {
    primary: {
      50: '{sky.50}',
      100: '{sky.100}',
      200: '{sky.200}',
      300: '{sky.300}',
      400: '{sky.400}',
      500: '{sky.500}',
      600: '{sky.600}',
      700: '{sky.700}',
      800: '{sky.800}',
      900: '{sky.900}',
      950: '{sky.950}',
    },
    colorScheme: {
      light: {
        surface: {
          0: '#F8F8FD',
          50: '{sky.50}',
          100: '{sky.100}',
          200: '{sky.200}',
          300: '{sky.300}',
          400: '{sky.400}',
          500: '{sky.500}',
          600: '{sky.600}',
          700: '{sky.700}',
          800: '{sky.800}',
          900: '{sky.900}',
          950: '{sky.950}',
        },
        text: { color: '#000000' },
      },
      dark: {
        surface: {
          0: '#FDFDFD',
          50: '#EAEDEF',
          100: '#D4DAE0',
          200: '#B8C2CD',
          300: '#9DAAB9',
          400: '#8291A5',
          500: '#677892',
          600: '#4E5F7B',
          700: '#3D4B64',
          800: '#2D374E',
          900: '#2A323D',
          950: '#171E27',
        },
        text: { color: '#ffffff' },
      },
    },
  },
  components: {
    breadcrumb: {
      root: {
        padding: '0',
      },
    },
    button: {
      root: {
        label: {
          fontWeight: 'var(--tod-button-label-font-weight)',
        },
      },
    },
    card: {
      root: {
        shadow: 'var(--tod-card-shadow)',
      },
    },
    datatable: {
      bodyCell: {
        padding: 'var(--tod-datatable-body-cell-padding)',
      },
      columnTitle: {
        fontWeight: 'var(--tod-datatable-column-title-font-weight)',
      },
      footer: {
        background: 'var(--tod-datatable-footer-cell-background)',
      },
      footerCell: {
        background: 'var(--tod-datatable-footer-cell-background)',
      },
      header: {
        borderWidth: '0 0 1px 0',
      },
      headerCell: {
        padding: 'var(--tod-datatable-header-cell-padding)',
        selectedBackground: 'var(--p-datatable-header-cell-background)',
        selectedColor: 'var(--p-button-text-primary-color)',
        sm: {
          padding: 'var(--tod-datatable-header-cell-padding)',
        },
      },
    },
    dialog: {
      header: {
        padding: '.8rem 1.25rem',
      },
    },
    drawer: {
      header: {
        padding: '0.5rem 0',
      },
      content: {
        padding: '0 0.5rem',
      },
    },
    selectbutton: {
      root: {
        borderRadius: 'var(--tod-selectbutton-border-radius)',
      },
    },
    tabs: {
      tab: {
        padding: 'var(--tod-tabs-tab-padding)',
      },
    },
    tag: {
      root: {
        fontSize: 'var(--tod-tag-font-size)',
        fontWeight: 'var(--tod-tag-font-weight)',
      },
      primary: {
        color: 'var(--p-primary-contrast-color)',
      },
    },
    tieredmenu: {
      item: {
        color: 'var(--p-primary-color)',
        icon: {
          color: 'var(--p-primary-icon-color)',
        },
      },
    },
    togglebutton: {
      content: {
        checkedBackground: 'var(--p-button-text-primary-color)',
        checkedShadow: '0px 1px 2px 0 rgba(0, 0, 0, 0.6)',
        borderRadius: '4px',
        padding: '0.25rem 0.25rem',
      },
      root: {
        background: 'var(--tod-togglebutton-background-border-color)',
        checkedBackground: 'var(--tod-togglebutton-background-border-color)',
        borderColor: 'var(--tod-togglebutton-background-border-color)',
        checkedBorderColor: 'var(--tod-togglebutton-background-border-color)',
        transitionDuration: '.5s',
        borderRadius: '4px',
        fontWeight: '500',
        padding: '.20rem .20rem',
      },
    },
  },
});
