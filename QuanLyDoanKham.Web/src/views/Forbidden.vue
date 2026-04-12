<template>
  <div class="min-h-screen flex items-center justify-center bg-[#f0f9ff] p-6 relative overflow-hidden">
    <!-- Decorative blobs -->
    <div class="absolute top-0 right-0 -mr-20 -mt-20 w-80 h-80 bg-blue-400/10 rounded-full blur-3xl animate-pulse"></div>
    <div class="absolute bottom-0 left-0 -ml-20 -mb-20 w-80 h-80 bg-red-400/10 rounded-full blur-3xl animate-pulse" style="animation-delay: 1s"></div>

    <div class="w-full max-w-2xl bg-white rounded-[4rem] neo-shadow overflow-hidden flex flex-col items-center relative z-10 border border-slate-100 p-16 md:p-24 animate-scale-up">
      <!-- Header -->
      <div class="flex flex-col items-center mb-16">
        <div class="bg-rose-500 p-6 rounded-[2rem] shadow-2xl shadow-rose-500/20 mb-8 rotate-3">
          <ShieldAlert class="text-white w-16 h-16" />
        </div>
        <h1 class="text-7xl font-black text-slate-900 mb-4 italic">403</h1>
        <h2 class="text-3xl font-black text-rose-600 mb-6 uppercase tracking-widest">Truy cập bị hạn chế</h2>
        <p class="text-slate-400 font-black text-center max-w-sm leading-relaxed px-4">
          Phát hiện yêu cầu truy xuất trái phép hoặc thiếu đặc quyền thực thi. Vui lòng liên hệ trung tâm điều hành nếu bạn cho rằng đây là lỗi phân quyền.
        </p>
      </div>

      <!-- Info box -->
      <div class="w-full bg-slate-50 border border-slate-100 rounded-[2.5rem] p-8 mb-12 relative overflow-hidden group">
        <div class="absolute -right-4 -top-4 w-20 h-20 bg-indigo-500/5 rounded-full blur-2xl group-hover:scale-150 transition-transform"></div>
        <div class="flex items-center space-x-6 relative z-10">
          <div class="bg-indigo-50 p-4 rounded-2xl text-indigo-500">
             <AlertCircle class="w-8 h-8 flex-shrink-0" />
          </div>
          <div>
            <p class="text-[10px] font-black text-slate-400 uppercase tracking-[0.2em] mb-1">Mức độ định danh hiện tại</p>
            <p class="text-lg font-black text-slate-900 uppercase tracking-widest">VAI TRÒ: <span class="text-indigo-600">{{ authStore.userRole || 'GUEST' }}</span></p>
          </div>
        </div>
      </div>

      <!-- Actions -->
      <div class="flex flex-col sm:flex-row gap-6 w-full">
        <button
          @click="goBack"
          class="flex-1 btn-premium border-2 border-slate-200 text-slate-600 hover:bg-slate-50 py-6"
        >
          <ArrowLeft class="w-6 h-6" />
          <span>QUAY LẠI</span>
        </button>
        <button
          @click="goDashboard"
          class="flex-1 btn-premium bg-indigo-600 text-white shadow-indigo-600/20 py-6"
        >
          <Home class="w-6 h-6" />
          <span>VỀ TRUNG TÂM</span>
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { ShieldAlert, AlertCircle, ArrowLeft, Home } from 'lucide-vue-next'

const router = useRouter()
const authStore = useAuthStore()

const goBack = () => router.back()
const goDashboard = () => router.push('/')
</script>
