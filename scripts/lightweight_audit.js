const fs = require('fs');
const path = require('path');

const tags = ['Teleport', 'div', 'Transition', 'template', 'main', 'header', 'footer', 'form', 'button', 'span', 'p'];

function checkTags(filePath) {
  const content = fs.readFileSync(filePath, 'utf8');
  // Simple check for unclosed Teleport tags
  const openCount = (content.match(/<Teleport/g) || []).length;
  const closeCount = (content.match(/<\/Teleport>/g) || []).length;
  
  if (openCount !== closeCount) {
    return { 'Teleport': { openCount, closeCount } };
  }
  
  return null;
}

function walkDir(dir) {
  const files = fs.readdirSync(dir);
  files.forEach(file => {
    const fullPath = path.join(dir, file);
    if (fs.statSync(fullPath).isDirectory()) {
      if (file !== 'node_modules') walkDir(fullPath);
    } else if (file.endsWith('.vue')) {
      const issues = checkTags(fullPath);
      if (issues) {
        process.stdout.write(`Issue in ${fullPath}: ${JSON.stringify(issues, null, 2)}\n`);
      }
    }
  });
}

const srcPath = 'd:/QuanLyDoanKham/QuanLyDoanKham.Web/src';
walkDir(srcPath);
process.stdout.write('Lightweight Audit completed.\n');
