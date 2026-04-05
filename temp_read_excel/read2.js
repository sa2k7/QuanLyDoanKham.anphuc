const XLSX = require('xlsx');
const workbook1 = XLSX.readFile('d:/QuanLyDoanKham/docs/Test_Reports/DSDOANTHEONGAY-03-04-2024.xlsx');
console.log("=== DSDOANTHEONGAY table headers ===");
console.log(XLSX.utils.sheet_to_json(workbook1.Sheets[workbook1.SheetNames[0]], {header:1, defval: null}).slice(10, 20));
