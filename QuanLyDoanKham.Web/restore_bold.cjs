const fs = require('fs');
const path = require('path');

function processDir(dir) {
    const items = fs.readdirSync(dir);
    for (const item of items) {
        const fullPath = path.join(dir, item);
        const stat = fs.statSync(fullPath);
        if (stat.isDirectory()) {
            processDir(fullPath);
        } else if (fullPath.endsWith('.vue') || fullPath.endsWith('.css')) {
            let content = fs.readFileSync(fullPath, 'utf-8');
            content = content.replace(/\bfont-bold\b/g, 'font-black');
            
            // Re-add tracking specifically for standard labels
            // (Only basic heuristic replacement to bring back the vibe without blowing out the layout)
            content = content.replace(/class="([^"]*)\buppercase\b([^"]*)"/g, (match, p1, p2) => {
                if (!p1.includes('tracking') && !p2.includes('tracking')) {
                     return `class="${p1}uppercase tracking-widest${p2}"`;
                }
                return match;
            });
            fs.writeFileSync(fullPath, content);
        }
    }
}

processDir(path.join(__dirname, 'src'));
console.log('Restored font-black and tracking-widest to uppercase elements.');
