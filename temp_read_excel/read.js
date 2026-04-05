const XLSX = require('xlsx');
const workbook1 = XLSX.readFile('d:/QuanLyDoanKham/docs/Test_Reports/DSDOANTHEONGAY-03-04-2024.xlsx');
console.log("=== DSDOANTHEONGAY ===");
console.log(XLSX.utils.sheet_to_json(workbook1.Sheets[workbook1.SheetNames[0]], {header:1, defval: null}).slice(0, 10));

const workbook2 = XLSX.readFile('d:/QuanLyDoanKham/docs/Test_Reports/THEO DÕI HĐ KSK NGOẠI TUYẾN HN2025.xlsx');
console.log("=== THEO DOI HD ===");
console.log(XLSX.utils.sheet_to_json(workbook2.Sheets[workbook2.SheetNames[0]], {header:1, defval: null}).slice(0, 10));
