/**
 * errorHelper.js - Tiện ích phân tích lỗi từ Backend
 *
 * Chuẩn hóa error messages theo HTTP status code (Requirements 4.2, 4.3, 4.4, 4.5)
 */

export function parseApiError(error, fallback = 'Đã xảy ra lỗi') {
  if (!error) return fallback
  const status = error?.response?.status
  const serverMessage = error?.response?.data?.message || error?.response?.data
  if (status === 403) return 'Bạn không có quyền thực hiện thao tác này'
  if (status === 404) return 'Không tìm thấy dữ liệu'
  if (status >= 500) return 'Lỗi hệ thống, vui lòng thử lại sau'
  if (!error?.response) return 'Không thể kết nối tới server'
  if (typeof serverMessage === 'string') return serverMessage
  return fallback
}
