<template>
  <div class="min-h-screen flex items-center justify-center font-sans relative overflow-hidden bg-slate-50">
    <!-- Mesh Gradient Background & Blur Orbs -->
    <div class="absolute top-[-20%] right-[-10%] w-[50%] h-[50%] bg-emerald-400/10 rounded-full blur-[100px] pointer-events-none"></div>
    <div class="absolute bottom-[-10%] left-[-10%] w-[40%] h-[40%] bg-indigo-500/10 rounded-full blur-[100px] pointer-events-none"></div>
    <div class="absolute top-[20%] left-[-20%] w-[30%] h-[30%] bg-teal-300/10 rounded-full blur-[100px] pointer-events-none"></div>

    <!-- Authentic Glassmorphism Main Card (Enhanced Border & Shadow for popup effect) -->
    <div class="w-full max-w-[28rem] bg-white/95 backdrop-blur-2xl border border-slate-300 shadow-[0_20px_40px_-15px_rgba(0,0,0,0.15)] rounded-[2.5rem] p-12 relative z-10 mx-4 animate-slide-up">
      <!-- Header -->
      <div class="text-center mb-10">
        <!-- Softer Logo Box -->
        <div class="inline-flex items-center justify-center w-20 h-20 bg-white rounded-2xl mb-6 shadow-sm border border-slate-100/50 p-2 transform hover:scale-105 transition-transform duration-300">
          <img :src="logo" class="w-16 h-16 object-contain drop-shadow-sm" alt="Logo" />
        </div>
        <h2 class="text-2xl font-bold text-slate-800 tracking-tight">Đa Khoa An Phúc</h2>
        <p class="text-slate-500 font-medium uppercase tracking-widest text-[10px] mt-2">Quản lý Đoàn khám</p>
      </div>

      <form @submit.prevent="handleLogin" autocomplete="off" class="space-y-5">
        <!-- iOS-style Input: Email / Username -->
        <div class="relative group">
          <div class="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none text-slate-400 group-focus-within:text-primary transition-colors duration-300">
            <User class="w-[18px] h-[18px]" />
          </div>
          <input 
            v-model="username"
            type="text" 
            autocomplete="off"
            class="w-full h-[54px] pl-12 pr-4 rounded-2xl bg-slate-50/50 border border-slate-200/80 hover:border-primary/30 focus:border-primary focus:ring-4 focus:ring-primary/10 focus:bg-white outline-none font-semibold text-slate-800 transition-all duration-300 placeholder:text-slate-400 placeholder:font-normal text-[14px]"
            placeholder="Email hoặc Tên đăng nhập"
            required
          />
        </div>

        <!-- iOS-style Input: Password -->
        <div class="relative group">
          <div class="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none text-slate-400 group-focus-within:text-primary transition-colors duration-300">
            <Lock class="w-[18px] h-[18px]" />
          </div>
          <input 
            v-model="password"
            @input="password = $event.target.value.replace(/[^\x00-\x7F]/g, '')"
            :type="showPassword ? 'text' : 'password'" 
            autocomplete="new-password"
            class="w-full h-[54px] pl-12 pr-12 rounded-2xl bg-slate-50/50 border border-slate-200/80 hover:border-primary/30 focus:border-primary focus:ring-4 focus:ring-primary/10 focus:bg-white outline-none font-semibold text-slate-800 transition-all duration-300 placeholder:text-slate-400 placeholder:font-normal text-[14px]"
            placeholder="Mật khẩu bảo mật"
            required
          />
          <button 
            type="button"
            @click="showPassword = !showPassword"
            class="absolute inset-y-0 right-0 pr-4 flex items-center text-slate-400 hover:text-primary transition-colors duration-300 outline-none"
          >
            <Eye v-if="!showPassword" class="w-[18px] h-[18px]" />
            <EyeOff v-else class="w-[18px] h-[18px]" />
          </button>
        </div>

        <!-- Settings Options: iOS Toggle & Forgot Password -->
        <div class="flex items-center justify-between px-1 pt-1 pb-2">
          <!-- iOS Toggle Switch for Remember Me -->
          <label class="flex items-center cursor-pointer group/toggle gap-3">
            <div class="relative">
              <input v-model="rememberMe" type="checkbox" class="sr-only peer">
              <div class="w-9 h-5 bg-slate-200 peer-focus:outline-none rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-4 after:w-4 after:transition-all peer-checked:bg-emerald-500 shadow-inner"></div>
            </div>
            <span class="text-[13px] font-medium text-slate-500 group-hover/toggle:text-slate-700 transition-colors select-none">Lưu đăng nhập</span>
          </label>

          <button 
            type="button" 
            @click="showForgotModal = true"
            class="text-[13px] font-semibold text-slate-500 hover:text-emerald-600 transition-colors duration-300"
          >
            Quên mật khẩu?
          </button>
        </div>

        <transition name="fade">
          <div v-if="authStore.error" class="bg-rose-50/80 backdrop-blur-sm text-rose-600 p-4 rounded-xl text-[13px] font-medium flex items-center gap-3 border border-rose-100/50 shadow-sm animate-shake">
              <AlertCircle class="w-[18px] h-[18px] flex-shrink-0" />
              <span>{{ authStore.error }}</span>
          </div>
        </transition>

        <!-- Big Glow Button -->
        <button 
          :disabled="authStore.loading"
          type="submit" 
          class="w-full bg-gradient-to-r from-emerald-600 to-teal-500 text-white hover:from-emerald-500 hover:to-teal-400 h-[56px] rounded-2xl shadow-[0_8px_20px_rgb(20,184,166,0.25)] hover:shadow-[0_12px_25px_rgb(20,184,166,0.4)] hover:-translate-y-0.5 transition-all duration-300 mt-2 relative overflow-hidden group"
        >
          <div class="absolute w-[200%] h-full bg-white/20 transform -translate-x-full group-hover:translate-x-full transition-transform duration-1000 ease-in-out -skew-x-12 disabled:hidden"></div>
          <div class="relative flex items-center justify-center gap-2">
            <span v-if="!authStore.loading" class="tracking-wide text-[14px] font-bold uppercase">Đăng nhập hệ thống</span>
            <Loader2 v-else class="w-5 h-5 animate-spin" />
            <ArrowRight v-if="!authStore.loading" class="w-[18px] h-[18px] group-hover:translate-x-1 transition-transform" />
          </div>
        </button>
      </form>

      <!-- Forgot Password Modal -->
      <Teleport to="body">
        <div v-if="showForgotModal" class="fixed inset-0 z-[1000] flex items-center justify-center p-6 bg-slate-900/20 backdrop-blur-sm animate-fade-in">
          <div class="bg-white/90 backdrop-blur-xl w-full max-w-sm rounded-[2rem] shadow-[0_8px_32px_0_rgba(31,38,135,0.07)] p-8 border border-white animate-scale-up text-center relative overflow-hidden">
              <div class="w-14 h-14 bg-emerald-50 text-emerald-600 rounded-full flex flex-col items-center justify-center mx-auto mb-5 shadow-sm">
                  <ShieldAlert class="w-6 h-6" />
              </div>
              <h3 class="text-lg font-bold text-slate-800 mb-2">Cấp lại mật khẩu</h3>
              <p class="text-[13px] font-medium text-slate-500 leading-relaxed mb-6 px-2">
                  Hệ thống sẽ gửi yêu cầu khôi phục đến số liên lạc của bạn.
              </p>
              
              <div class="flex flex-col gap-3 mb-6">
                  <input v-model="resetUsername" type="text" class="w-full text-center pl-4 pr-4 py-3 rounded-xl bg-slate-50 border border-slate-200 focus:border-emerald-500 focus:ring-2 focus:ring-emerald-500/10 focus:bg-white outline-none font-medium text-slate-700 transition-all text-sm" placeholder="Nhập Email / Username..." />
                  <p v-if="resetMessage" :class="['text-[11px] font-semibold ', resetSuccess ? 'text-emerald-500' : 'text-rose-500']">
                    {{ resetMessage }}
                  </p>
              </div>

              <div class="flex gap-3">
                  <button @click="showForgotModal = false; resetMessage = ''; resetUsername = ''" class="flex-1 py-3 bg-slate-100 hover:bg-slate-200 text-slate-600 rounded-xl font-semibold text-[12px] transition-colors tracking-wide">Hủy</button>
                  <button @click="submitResetRequest" :disabled="isResetting" class="flex-[1.5] py-3 bg-slate-800 hover:bg-slate-700 text-white rounded-xl font-semibold text-[12px] shadow-md disabled:opacity-50 transition-colors tracking-wide">
                    {{ isResetting ? 'ĐANG GỬI...' : 'GỬI YÊU CẦU' }}
                  </button>
              </div>
          </div>
        </div>
      </Teleport>

      <div class="mt-8 text-center">
          <p class="text-xs font-medium text-slate-400 tracking-wider">© 2026 AnPhuc Medical</p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import apiClient from '../services/apiClient'
import logo from '../assets/logo.svg'
import { 
  Stethoscope, 
  User, 
  Lock, 
  Eye,
  EyeOff,
  ArrowRight, 
  Loader2, 
  AlertCircle,
  ShieldAlert,
  Check
} from 'lucide-vue-next'

const router = useRouter()
const authStore = useAuthStore()

const username = ref('')
const password = ref('')
const rememberMe = ref(false)
const showPassword = ref(false)
const showForgotModal = ref(false)

// Reset Password Request State
const resetUsername = ref('')
const isResetting = ref(false)
const resetMessage = ref('')
const resetSuccess = ref(false)

const submitResetRequest = async () => {
  if (!resetUsername.value) {
    resetMessage.value = 'Vui lòng nhập Username'
    resetSuccess.value = false
    return
  }
  
  isResetting.value = true
  resetMessage.value = ''
  
  try {
    const res = await apiClient.post('/api/Auth/request-reset', { username: resetUsername.value })
    resetMessage.value = res.data.message
    resetSuccess.value = true
    setTimeout(() => {
      showForgotModal.value = false
      resetMessage.value = ''
      resetUsername.value = ''
    }, 3000)
  } catch (e) {
    resetMessage.value = e.response?.data?.message || e.response?.data || 'Lỗi khi gửi yêu cầu'
    resetSuccess.value = false
  } finally {
    isResetting.value = false
  }
}

const handleLogin = async () => {
  // Làm sạch input: bỏ khoảng trắng và bỏ dấy @ nếu lỡ copy từ bảng
  let cleanUsername = username.value.trim()
  if (cleanUsername.startsWith('@')) {
    cleanUsername = cleanUsername.substring(1)
  }
  
  const success = await authStore.login({ 
    username: cleanUsername, 
    password: password.value 
  })
  if (success) {
    router.push('/')
  }
}
</script>

<style scoped>
.animate-slide-up {
  animation: slideUp 0.6s ease-out forwards;
}

@keyframes slideUp {
  from {
    opacity: 0;
    transform: translateY(20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.fade-enter-active, .fade-leave-active {
  transition: opacity 0.3s ease;
}
.fade-enter-from, .fade-leave-to {
  opacity: 0;
}
</style>
