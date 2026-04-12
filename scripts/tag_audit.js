const fs = require('fs');
const path = require('path');

function checkTags(filePath, tagsToCheck) {
  const content = fs.readFileSync(filePath, 'utf8');
  const results = {};
  
  tagsToCheck.forEach(tag => {
    const openTagRegex = new RegExp(`<${tag}(\\s|>)`, 'g');
    const closeTagRegex = new RegExp(`</${tag}>`, 'g');
    
    const openCount = (content.match(openTagRegex) || []).length;
    const closeCount = (content.match(closeTagRegex) || []).length;
    
    if (openCount !== closeCount) {
      results[tag] = { openCount, closeCount };
    }
  });
  
  return Object.keys(results).length > 0 ? results : null;
}

function walkDir(dir, tags) {
  const files = fs.readdirSync(dir);
  files.forEach(file => {
    const fullPath = path.join(dir, file);
    if (fs.statSync(fullPath).isDirectory()) {
      if (file !== 'node_modules') walkDir(fullPath, tags);
    } else if (file.endsWith('.vue')) {
      const issues = checkTags(fullPath, tags);
      if (issues) {
        console.log(`Issue in ${fullPath}:`, JSON.stringify(issues, null, 2));
      }
    }
  });
}

const tags = ['Teleport', 'div', 'Transition', 'template', 'main', 'header', 'footer', 'form'];
try {
  walkDir('d:/QuanLyDoanKham/QuanLyDoanKham.Web/src', tags);
  console.log('Audit completed.');
} catch (e) {
  console.error(e);
}
