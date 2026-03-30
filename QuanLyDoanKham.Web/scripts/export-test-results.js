import fs from 'fs';
import path from 'path';

// Đường dẫn file kết quả
const jsonPath = path.resolve('test-results/results.json');
const csvPath = path.resolve('BAO_CAO_KIEM_THU.csv');

// Kiểm tra file JSON có tồn tại không
if (!fs.existsSync(jsonPath)) {
    console.error('❌ Không tìm thấy file kết quả robot (results.json).');
    process.exit(1);
}

const data = JSON.parse(fs.readFileSync(jsonPath, 'utf8'));

// Tiêu đề cột
const headers = ['STT', 'Ten Kich Ban', 'Ket Qua', 'Thoi Gian (ms)', 'Loi Chinh'];
const rows = [headers];

let index = 1;
data.suites.forEach(suite => {
    suite.specs.forEach(spec => {
        spec.tests.forEach(test => {
            test.results.forEach(result => {
                const title = `${suite.title} > ${spec.title}`;
                const status = result.status.toUpperCase();
                const duration = result.duration;
                const error = result.error ? result.error.message.replace(/[\n\r,]/g, ' ') : '';
                
                rows.push([index++, title, status, duration, error]);
            });
        });
    });
});

// Chuyển mảng thành định dạng CSV (Dấu phẩy ngăn cách)
const csvContent = rows.map(r => r.join(',')).join('\n');

// Ghi file với BOM để Excel nhận diện được UTF-8 (Tiếng Việt)
fs.writeFileSync(csvPath, '\ufeff' + csvContent, 'utf8');

console.log(`✅ Đã xuất báo cáo Excel (CSV) tại: ${csvPath}`);
