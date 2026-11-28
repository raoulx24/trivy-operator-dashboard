Example package.json:

```json
"scripts": {
  "postinstall": "node ./scripts/fix-tailwindcss-primeui.js"
}
```

2.	Add a file named fix-tailwindcss-primeui.js 
```js
const fs = require('fs');
const path = require('path');

const packageJsonPath = path.resolve(
  __dirname,
  '../node_modules/tailwindcss-primeui/package.json'
);

try {
  const packageJson = JSON.parse(fs.readFileSync(packageJsonPath, 'utf8'));
  packageJson.type = 'module';
  fs.writeFileSync(packageJsonPath, JSON.stringify(packageJson, null, 2));
  console.log('Added "type": "module" to tailwindcss-primeui/package.json');
} catch (error) {
  console.error('Failed to update tailwindcss-primeui/package.json:', error);
}
```
