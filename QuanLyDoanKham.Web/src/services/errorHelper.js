/**
 * errorHelper.js - Tiá»‡n Ă­ch phĂ¢n tĂ­ch lá»—i tá»« Backend (ASP.NET Core Validation)
 */

export const parseApiError = (error) => {
    if (!error.response) {
        return error.message || "KhĂ´ng thá»ƒ káº¿t ná»‘i tá»›i mĂ¡y chá»§";
    }

    const { status, data } = error.response;

    // TrÆ°á» ng há»£p lá»—i Validation (400 Bad Request) theo chuáº©n ASP.NET Core
    if (status === 400 && data.errors) {
        const errorMessages = [];
        for (const key in data.errors) {
            if (Object.hasOwnProperty.call(data.errors, key)) {
                const messages = data.errors[key];
                if (Array.isArray(messages)) {
                    errorMessages.push(`${key}: ${messages.join(', ')}`);
                } else {
                    errorMessages.push(`${key}: ${messages}`);
                }
            }
        }
        return errorMessages.join('\n') || data.title || "Dá»¯ liá»‡u khĂ´ng há»£p lá»‡";
    }

    // Các trường hợp lỗi khác (trả về string trực tiếp hoặc message)
    if (typeof data === 'string') {
        // Bảo mật: Nếu string quá dài (ví dụ stack trace) hoặc chứa ký tự Token, không hiện trực tiếp
        if (data.length > 1000 || data.includes('ey')) {
            return `Lỗi hệ thống hoặc thiếu quyền thực thi (${status})`;
        }
        return data;
    }
    
    if (data.message) return data.message;
    if (data.title) return data.title;

    return `Lỗi hệ thống (${status})`;
};
