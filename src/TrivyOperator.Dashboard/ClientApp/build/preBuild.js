const $RefParser = require('@apidevtools/json-schema-ref-parser');
const NgOpenApiGen = require('ng-openapi-gen').NgOpenApiGen;

generateApi();

async function generateApi() {
  const options = {
    input: 'backend-api.yaml',
    output: 'src/api',
  };
  // load the openapi-spec and resolve all $refs
  const RefParser = new $RefParser();
  const openApi = await RefParser.bundle(options.input, {
    dereference: { circular: false },
  });

  patchRequiredFields(openApi);
  makePathsRelative(openApi);

  const ngOpenGen = new NgOpenApiGen(openApi, options);
  ngOpenGen.generate();
}

function patchRequiredFields(openApi) {
  const schemas = openApi.components?.schemas || {};

  for (const [schemaName, schema] of Object.entries(schemas)) {
    if (!schema.properties) continue;

    schema.required = schema.required || [];

    for (const [propName, propSchema] of Object.entries(schema.properties)) {
      const alreadyRequired = schema.required.includes(propName);
      const isNullable = propSchema.nullable === true;

      // Add all non-nullable properties to required
      if (!alreadyRequired && !isNullable) {
        schema.required.push(propName);
      }
    }
  }
}

function makePathsRelative(openApi) {
  const paths = openApi.paths || {};

  for (const path of Object.keys(paths)) {
    // Skip root path "/" because it becomes empty
    console.info(path)
    if (path === '/') continue;

    if (path.startsWith('/')) {
      const relative = path.slice(1); // remove leading slash
      paths[relative] = paths[path];
      delete paths[path];
    }
  }
}

