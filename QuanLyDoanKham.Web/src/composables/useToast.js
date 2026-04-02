import { ref, h, render } from 'vue'
import Toast from '../components/Toast.vue'

const toasts = ref([])

export function useToast() {
    const show = (message, type = 'success', duration = 4000) => {
        // Tự động giải nén message nếu là object từ API response
        let finalMessage = message
        if (typeof message === 'object' && message !== null) {
            finalMessage = message.message || message.title || JSON.stringify(message)
        }

        const container = document.createElement('div')
        document.body.appendChild(container)

        const toastId = Date.now()

        const close = () => {
            render(null, container)
            document.body.removeChild(container)
            toasts.value = toasts.value.filter(t => t.id !== toastId)
        }

        const vnode = h(Toast, {
            message: finalMessage,
            type,
            duration,
            onClose: close
        })

        render(vnode, container)
        toasts.value.push({ id: toastId, container })
    }

    const success = (message, duration) => show(message, 'success', duration)
    const error = (message, duration) => show(message, 'error', duration)
    const warning = (message, duration) => show(message, 'warning', duration)
    const info = (message, duration) => show(message, 'info', duration)

    return {
        show,
        success,
        error,
        warning,
        info
    }
}
