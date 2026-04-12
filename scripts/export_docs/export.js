const fs = require('fs');
const path = require('path');
const ExcelJS = require('exceljs');
const { Document, Packer, Paragraph, Table, TableRow, TableCell, WidthType, HeadingLevel, TextRun, AlignmentType } = require('docx');

const baseDir = 'd:/QuanLyDoanKham';
const tcMd = path.join(baseDir, 'docs/test_cases.md');
const tcXlsx = path.join(baseDir, 'docs/TEST_CASES.xlsx');
const tcDocx = path.join(baseDir, 'docs/TEST_CASES.docx');

const sysMd = path.join(baseDir, 'docs/SYSTEM_DOCUMENTATION.md');
const sysDocx = path.join(baseDir, 'docs/SYSTEM_DOCUMENTATION.docx');

async function exportTestCases() {
    console.log('--- Exporting Test Cases ---');
    const content = fs.readFileSync(tcMd, 'utf8');
    
    const modules = [];
    let currentModule = null;
    let inTable = false;
    let headers = [];

    const lines = content.split('\n');
    for (const line of lines) {
        if (line.startsWith('## MODULE')) {
            currentModule = {
                name: line.replace('## ', '').trim(),
                rows: []
            };
            modules.push(currentModule);
            inTable = false;
        } else if (line.trim().startsWith('|') && line.includes('|---|')) {
            inTable = true;
        } else if (inTable && line.trim().startsWith('|')) {
            const cells = line.split('|').map(c => c.trim()).filter((c, i, a) => i > 0 && i < a.length - 1);
            if (cells[0] === 'TC#') {
                headers = cells;
            } else if (!line.includes('|---|')) {
                currentModule.rows.push(cells);
            }
        } 
    }

    // Excel
    const workbook = new ExcelJS.Workbook();
    const sheet = workbook.addWorksheet('Test Cases');
    sheet.columns = headers.map(h => ({ header: h, key: h, width: 25 }));
    modules.forEach(mod => {
        sheet.addRow([mod.name]);
        const lastRow = sheet.lastRow;
        lastRow.font = { bold: true, size: 14 };
        sheet.mergeCells(lastRow.number, 1, lastRow.number, headers.length);
        mod.rows.forEach(row => sheet.addRow(row));
        sheet.addRow([]);
    });
    sheet.getRow(1).font = { bold: true };
    sheet.getRow(1).fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFE0E0E0' } };
    await workbook.xlsx.writeFile(tcXlsx);
    console.log('Saved: ' + tcXlsx);

    // Word
    const docSections = [
        new Paragraph({
            text: "BẢNG TEST CASE CHI TIẾT - QuanLyDoanKham",
            heading: HeadingLevel.HEADING_1,
            alignment: AlignmentType.CENTER
        })
    ];
    modules.forEach(mod => {
        docSections.push(new Paragraph({ text: mod.name, heading: HeadingLevel.HEADING_2, spacing: { before: 400, after: 200 } }));
        const tableRows = [
            new TableRow({
                children: headers.map(h => new TableCell({
                    children: [new Paragraph({ children: [new TextRun({ text: h, bold: true })] })],
                    shading: { fill: 'E0E0E0' }
                }))
            })
        ];
        mod.rows.forEach(row => {
            tableRows.push(new TableRow({ children: row.map(cell => new TableCell({ children: [new Paragraph(cell)] })) }));
        });
        docSections.push(new Table({ rows: tableRows, width: { size: 100, type: WidthType.PERCENTAGE } }));
    });
    const doc = new Document({ sections: [{ children: docSections }] });
    fs.writeFileSync(tcDocx, await Packer.toBuffer(doc));
    console.log('Saved: ' + tcDocx);
}

async function exportSystemDoc() {
    console.log('--- Exporting System Documentation ---');
    const content = fs.readFileSync(sysMd, 'utf8');
    const lines = content.split('\n');
    const docChildren = [
        new Paragraph({
            text: "TÀI LIỆU TỔNG HỢP HỆ THỐNG - QuanLyDoanKham",
            heading: HeadingLevel.HEADING_1,
            alignment: AlignmentType.CENTER
        })
    ];

    let inCode = false;
    let inTable = false;
    let tableData = [];

    for (let line of lines) {
        if (line.startsWith('```')) {
            inCode = !inCode;
            continue;
        }

        if (inCode) {
            docChildren.push(new Paragraph({
                children: [new TextRun({ text: line, font: 'Courier New', size: 18 })],
                spacing: { after: 0 }
            }));
            continue;
        }

        if (line.trim().startsWith('|') && line.includes('|---|')) {
            inTable = true;
            continue;
        }

        if (inTable && line.trim().startsWith('|')) {
            const cells = line.split('|').map(c => c.trim()).filter((c, i, a) => i > 0 && i < a.length - 1);
            tableData.push(cells);
            continue;
        }

        if (inTable && !line.trim().startsWith('|')) {
            // Write table
            const tableRows = tableData.map((row, idx) => new TableRow({
                children: row.map(cell => new TableCell({
                    children: [new Paragraph({ children: [new TextRun({ text: cell, bold: idx === 0 })] })],
                    shading: idx === 0 ? { fill: 'F0F0F0' } : undefined
                }))
            }));
            docChildren.push(new Table({ rows: tableRows, width: { size: 100, type: WidthType.PERCENTAGE } }));
            docChildren.push(new Paragraph({ text: "" }));
            inTable = false;
            tableData = [];
        }

        if (!inTable) {
            if (line.startsWith('## ')) {
                docChildren.push(new Paragraph({ text: line.replace('## ', ''), heading: HeadingLevel.HEADING_2, spacing: { before: 240, after: 120 } }));
            } else if (line.startsWith('### ')) {
                docChildren.push(new Paragraph({ text: line.replace('### ', ''), heading: HeadingLevel.HEADING_3, spacing: { before: 200, after: 100 } }));
            } else if (line.trim().length > 0) {
                docChildren.push(new Paragraph({ text: line.trim(), spacing: { after: 100 } }));
            }
        }
    }

    const doc = new Document({ sections: [{ children: docChildren }] });
    fs.writeFileSync(sysDocx, await Packer.toBuffer(doc));
    console.log('Saved: ' + sysDocx);
}

async function main() {
    await exportTestCases();
    await exportSystemDoc();
    console.log('All exports completed!');
}

main().catch(console.error);
