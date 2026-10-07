const angularEslint = require('@angular-eslint/eslint-plugin');
const angularEslintTemplate = require('@angular-eslint/eslint-plugin-template');
const typescriptPlugin = require('@typescript-eslint/eslint-plugin');
const html = require('eslint-plugin-html');
const parser = require('@typescript-eslint/parser');

module.exports = [
  {
    ignores: ['.angular', 'dist', 'node_modules', 'src/api'],
  },
  {
    files: ['**/*.ts'],
    languageOptions: {
      parser,
      parserOptions: {
        project: ['**/tsconfig.json'],
        createDefaultProgram: true,
      },
    },
    plugins: {
      '@angular-eslint': angularEslint,
      '@typescript-eslint': typescriptPlugin,
    },
    rules: {
      '@angular-eslint/component-selector': [
        'error',
        {
          prefix: 'app',
          style: 'kebab-case',
          type: 'element',
        },
      ],
      '@angular-eslint/directive-selector': [
        'error',
        {
          prefix: 'app',
          style: 'camelCase',
          type: 'attribute',
        },
      ],
      '@typescript-eslint/no-empty-function': [
        'error',
        {
          allow: ['arrowFunctions'],
        },
      ],
      'prefer-const': 'error',
      'no-var': 'error',
    },
  },
  {
    files: ['**/*.html'],
    languageOptions: {
      parser: require('@angular-eslint/template-parser'),
    },
    plugins: {
      html: html,
      '@angular-eslint/template': angularEslintTemplate,
    },
    rules: {},
  },
];
