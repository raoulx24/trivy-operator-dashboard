/** @type {import('prettier-plugin-organize-attributes').PrettierPluginOrganizeAttributesParserOptions} **/
const angularPrettierPluginOrganizeAttributesParserOptions = {
  attributeGroups: [
    '$ANGULAR_STRUCTURAL_DIRECTIVE',
    '$ANGULAR_ELEMENT_REF',
    '$ID',
    '$DEFAULT',
    '$CLASS',
    '$ANGULAR_INPUT',
    '$ANGULAR_TWO_WAY_BINDING',
    '$ANGULAR_OUTPUT',
    '$ANGULAR_ANIMATION',
    '$ANGULAR_ANIMATION_INPUT',
  ],
  attributeSort: 'ASC',
  attributeIgnoreCase: true,
};

/** @type {import('prettier').Options} **/
module.exports = {
  printWidth: 120,
  tabWidth: 2,
  useTabs: false,
  singleQuote: true,
  trailingComma: 'all',
  plugins: [
    'prettier-plugin-organize-attributes',
    'prettier-plugin-organize-imports',
    // should be last
    'prettier-plugin-multiline-arrays',
  ],
  overrides: [
    {
      files: ['*.html'],
      options: {
        parser: 'angular',
        ...angularPrettierPluginOrganizeAttributesParserOptions,
      },
    },
    {
      files: ['index.html'],
      options: {
        parser: 'html',
      },
    },
  ],
};
