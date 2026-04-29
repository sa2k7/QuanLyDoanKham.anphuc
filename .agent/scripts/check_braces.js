
const fs = require('fs');
const content = fs.readFileSync('d:\\QuanLyDoanKham-NEW\\QuanLyDoanKham.API\\Migrations\\ApplicationDbContextModelSnapshot.cs', 'utf8');
let balance = 0;
const lines = content.split('\n');
for (let i = 0; i < lines.length; i++) {
    const line = lines[i];
    for (let j = 0; j < line.length; j++) {
        if (line[j] === '{') balance++;
        if (line[j] === '}') balance--;
        if (balance < 0) {
            console.log(`Balance dropped below 0 at line ${i + 1}: ${line.trim()}`);
            process.exit(1);
        }
    }
}
