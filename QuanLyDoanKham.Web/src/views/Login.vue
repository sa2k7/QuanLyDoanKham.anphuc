<template>
  <div class="min-h-screen flex items-center justify-center bg-slate-50 p-6 font-sans relative overflow-hidden">
    <!-- Decorative background elements -->
    <div class="absolute top-[-10%] right-[-10%] w-[40%] h-[40%] bg-primary/5 rounded-full blur-[120px]"></div>
    <div class="absolute bottom-[-10%] left-[-10%] w-[40%] h-[40%] bg-indigo-500/5 rounded-full blur-[120px]"></div>

    <div class="w-full max-w-[28rem] bg-white rounded-[3rem] shadow-2xl p-12 border-2 border-slate-50 animate-slide-up relative z-10">
      <div class="text-center mb-12">
        <div class="inline-flex items-center justify-center w-20 h-20 bg-white rounded-[1.8rem] mb-6 shadow-2xl shadow-primary/10 transform -rotate-3 p-2 border border-slate-100">
          <img :src="logo" class="w-16 h-16 object-contain" alt="Logo" />
        </div>
        <h2 class="text-3xl font-black text-slate-900 uppercase">Đa Khoa An Phúc</h2>
        <p class="text-slate-400 font-black uppercase tracking-[0.2em] text-[10px] mt-4">Hệ điều hành Quản lý Đoàn khám</p>
      </div>

      <form @submit.prevent="handleLogin" autocomplete="off" class="space-y-8">
        <div class="flex flex-col gap-3">
          <label class="text-[10px] font-black uppercase tracking-[0.2em] text-slate-400 ml-1">Email hoặc Tên đăng nhập</label>
          <div class="relative group">
            <span class="absolute inset-y-0 left-0 pl-6 flex items-center text-slate-300 group-focus-within:text-primary transition-colors">
              <User class="w-5 h-5" />
            </span>
            <input 
              v-model="username"
              type="text" 
              autocomplete="off"
              class="w-full pl-14 pr-6 py-5 rounded-2xl bg-slate-50 border-2 border-transparent focus:border-primary/20 focus:bg-white outline-none font-black text-slate-600 transition-all shadow-inner group-hover:bg-slate-100/50"
              placeholder="vidu@gmail.com hoặc username..."
              required
            />
          </div>
        </div>

        <div class="flex flex-col gap-3">
          <label class="text-[10px] font-black uppercase tracking-[0.2em] text-slate-400 ml-1">Mật khẩu bảo mật</label>
          <div class="relative group">
            <span class="absolute inset-y-0 left-0 pl-6 flex items-center text-slate-300 group-focus-within:text-primary transition-colors">
              <Lock class="w-5 h-5" />
            </span>
            <input 
              v-model="password"
              @input="password = $event.target.value.replace(/[^\x00-\x7F]/g, '')"
              :type="showPassword ? 'text' : 'password'" 
              autocomplete="new-password"
              class="w-full pl-14 pr-14 py-5 rounded-2xl bg-slate-50 border-2 border-transparent focus:border-primary/20 focus:bg-white outline-none font-black text-slate-600 transition-all shadow-inner group-hover:bg-slate-100/50"
              placeholder="••••••••"
              required
            />
            <button 
              type="button"
              @click="showPassword = !showPassword"
              class="absolute inset-y-0 right-0 pr-6 flex items-center text-slate-300 hover:text-primary transition-colors"
            >
              <Eye v-if="!showPassword" class="w-5 h-5" />
              <EyeOff v-else class="w-5 h-5" />
            </button>
          </div>
        </div>

        <div class="flex items-center justify-between px-1">
          <label class="flex items-center gap-3 cursor-pointer group/check">
            <div class="relative flex items-center justify-center w-5 h-5 rounded-lg border-2 border-slate-200 group-hover/check:border-primary/50 transition-all overflow-hidden bg-slate-50">
              <input 
                v-model="rememberMe"
                type="checkbox" 
                class="peer absolute inset-0 opacity-0 cursor-pointer z-10"
              />
              <div class="w-full h-full bg-primary transform scale-0 peer-checked:scale-100 transition-transform flex items-center justify-center">
                <Check class="w-3 h-3 text-white" />
              </div>
            </div>
            <span class="text-[10px] font-black uppercase tracking-widest text-slate-400 group-hover/check:text-slate-600 transition-colors">Ghi nhớ tôi</span>
          </label>

          <button 
            type="button" 
            @click="showForgotModal = true"
            class="text-[10px] font-black uppercase tracking-widest text-slate-400 hover:text-primary transition-colors pr-2"
          >
            Quên mật khẩu?
          </button>
        </div>

        <transition name="fade">
          <div v-if="authStore.error" class="bg-rose-50 text-rose-600 p-5 rounded-2xl text-[11px] font-black uppercase tracking-widest flex items-center gap-3 border border-rose-100 animate-shake">
              <AlertCircle class="w-4 h-4 flex-shrink-0" />
              <span>{{ authStore.error }}</span>
          </div>
        </transition>

        <button 
          :disabled="authStore.loading"
          type="submit" 
          class="w-full btn-premium bg-slate-900 text-white hover:bg-black py-6 shadow-2xl shadow-slate-200"
        >
          <span v-if="!authStore.loading" class="tracking-[0.3em] text-xs uppercase font-black">Xác thực truy cập</span>
          <Loader2 v-else class="w-6 h-6 animate-spin" />
          <ArrowRight v-if="!authStore.loading" class="w-5 h-5 group-hover:translate-x-2 transition-transform" />
        </button>
      </form>

      <!-- Forgot Password Modal -->
      <Teleport to="body">
        <div v-if="showForgotModal" class="fixed inset-0 z-[1000] flex items-center justify-center p-6 bg-slate-900/40 backdrop-blur-sm animate-fade-in">
          <div class="bg-white w-full max-w-md rounded-[2.5rem] shadow-2xl p-10 border-2 border-slate-50 animate-scale-up text-center">
              <div class="w-16 h-16 bg-indigo-50 text-indigo-600 rounded-3xl flex items-center justify-center mx-auto mb-6">
                  <ShieldAlert class="w-8 h-8" />
              </div>
              <h3 class="text-xl font-black text-slate-800 mb-2">Yêu cầu cấp lại mật khẩu</h3>
              <p class="text-xs font-black text-slate-400 leading-relaxed mb-8">
                  Hệ thống sẽ gửi thông báo đến **Admin**. Vui lòng nhập thông tin tài khoản của bạn:
              </p>
              
              <div class="flex flex-col gap-4 mb-8">
                  <input v-model="resetUsername" type="text" class="input-premium w-full text-center" placeholder="Email hoặc Username..." />
                  <p v-if="resetMessage" :class="['text-[10px] font-black uppercase tracking-widest ', resetSuccess ? 'text-emerald-500' : 'text-rose-500']">
                    {{ resetMessage }}
                  </p>
              </div>

              <div class="flex gap-4">
                  <button @click="showForgotModal = false; resetMessage = ''; resetUsername = ''" class="flex-1 py-4 text-slate-400 font-black text-xs uppercase tracking-widest ">Hủy</button>
                  <button @click="submitResetRequest" :disabled="isResetting" class="flex-[2] py-4 bg-slate-900 text-white rounded-2xl font-black text-xs uppercase tracking-[0.2em] shadow-xl disabled:opacity-50">
                    {{ isResetting ? 'ĐANG GỬI...' : 'GỬI YÊU CẦU' }}
                  </button>
              </div>
          </div>
        </div>
      </Teleport>

      <div class="mt-12 text-center">
          <p class="text-[10px] font-black text-slate-300 uppercase tracking-[0.3em]">© 2026 HealthCare System</p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import axios from 'axios'
import logo from '../assets/logo.png'
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
    const res = await axios.post('http://localhost:5283/api/Auth/request-reset', { username: resetUsername.value })
    resetMessage.value = res.data.message
    resetSuccess.value = true
    setTimeout(() => {
      showForgotModal.value = false
      resetMessage.value = ''
      resetUsername.value = ''
    }, 3000)
  } catch (e) {
    resetMessage.value = e.response?.data || 'Lỗi khi gửi yêu cầu'
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
  
  const success = await authStore.login(cleanUsername, password.value, rememberMe.value)
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
