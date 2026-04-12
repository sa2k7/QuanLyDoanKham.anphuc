const fs = require('fs');
const path = require('path');

const tags = ['Teleport', 'div', 'Transition', 'template', 'main', 'header', 'footer', 'form', 'button', 'span', 'i', 'p', 'h1', 'h2', 'h3', 'h4', 'h5', 'h6', 'ul', 'li', 'table', 'tr', 'td', 'th', 'thead', 'tbody'];

function checkTags(filePath) {
  const content = fs.readFileSync(filePath, 'utf8');
  // Skip script and style blocks for tag checking
  const templateMatch = content.match(/<template>([\s\S]*)<\/template>/);
  if (!templateMatch) return null;
  const template = templateMatch[1];
  
  const results = {};
  
  tags.forEach(tag => {
    // Regex to find opening tags (ignoring self-closing ones like <div />)
    const openTagRegex = new RegExp(`<${tag}(\\s|[^/>])*[^/]>`, 'gi');
    // Regex to find closing tags
    const closeTagRegex = new RegExp(`</${tag}>`, 'gi');
    
    const openCount = (template.match(openTagRegex) || []).length;
    const closeCount = (template.match(closeTagRegex) || []).length;
    
    if (openCount !== closeCount) {
      results[tag] = { openCount, closeCount };
    }
  });
  
  return Object.keys(results).length > 0 ? results : null;
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
if (fs.existsSync(srcPath)) {
  walkDir(srcPath);
  process.stdout.write('Hardening Audit completed.\n');
} else {
  process.stderr.write(`Path not found: ${srcPath}\n`);
}
