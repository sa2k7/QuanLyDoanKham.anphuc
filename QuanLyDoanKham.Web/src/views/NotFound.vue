<script setup>
import { useRouter } from 'vue-router'
import { Home } from 'lucide-vue-next'

const router = useRouter()
</script>

<template>
  <div class="min-h-screen bg-slate-50 flex flex-col items-center justify-center p-4">
    <div class="text-center space-y-6 max-w-md">
      <h1 class="text-9xl font-black text-indigo-500">404</h1>
      <h2 class="text-3xl font-black text-slate-800 uppercase tracking-widest">Không tìm thấy trang</h2>
      <p class="text-slate-500 font-medium text-sm">
        Trang bạn đang cố truy cập không tồn tại hoặc đã bị di chuyển.
      </p>
      <button 
        @click="router.push('/')" 
        class="inline-flex items-center gap-2 bg-slate-900 text-white px-8 py-4 rounded-2xl font-black text-xs uppercase tracking-[0.2em] shadow-[4px_4px_0px_#1e293b] hover:-translate-y-1 hover:shadow-[6px_6px_0px_#1e293b] transition-all">
        <Home class="w-4 h-4" /> TRỞ VỀ TRANG CHỦ
      </button>
    </div>
  </div>
</template>
