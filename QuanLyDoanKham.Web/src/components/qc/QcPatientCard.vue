<template>
  <div class="patient-card bg-white/80 backdrop-blur-xl border border-slate-100 rounded-3xl p-6 shadow-lg">
    <div class="flex items-start justify-between">
      <div>
        <h2 class="text-2xl font-black text-slate-800">{{ fullName }}</h2>
        <div class="flex items-center gap-4 mt-2">
          <span class="text-xs font-bold text-slate-500">
            STT: <span class="text-indigo-600 text-base">#{{ queueNo }}</span>
          </span>
          <span class="px-3 py-1 bg-orange-100 text-orange-700 rounded-full text-xs font-black uppercase">
            Chờ QC
          </span>
        </div>
      </div>
      <div class="text-right">
        <p class="text-[10px] font-black text-slate-400 uppercase tracking-wider">Mã hồ sơ</p>
        <p class="text-2xl font-black text-slate-700">{{ medicalRecordId }}</p>
      </div>
    </div>
  </div>
</template>

<script setup>
defineProps({
  fullName: String,
  queueNo: [Number, String],
  medicalRecordId: [Number, String]
})
</script>
