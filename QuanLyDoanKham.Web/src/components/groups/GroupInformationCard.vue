<template>
  <div class="group-information-card premium-card bg-white rounded-[2rem] shadow-xl border border-slate-100 overflow-hidden group/card mb-8">
    <!-- Blue Header -->
    <div class="p-5 bg-primary text-white flex justify-between items-center transition-all">
        <div class="flex items-center gap-5">
            <div class="w-10 h-10 bg-white/20 backdrop-blur-md rounded-xl flex items-center justify-center text-white shadow-sm border border-white/10 shrink-0">
                <Stethoscope class="w-5 h-5" />
            </div>
            <div class="min-w-0 pr-4">
                <h4 class="text-base font-black truncate">{{ group.groupName }}</h4>
                <p class="text-[9px] font-black text-white/70 uppercase tracking-widest mt-0.5 truncate italic">
                    [HĐ-{{ group.healthContractId }}] • {{ group.shortName || group.companyName }} • {{ formatDate(group.examDate) }}
                </p>
            </div>
        </div>
        <div class="flex items-center gap-3">
            <span :class="['px-3 py-1 rounded-lg text-[8px] font-black uppercase tracking-tighter border border-white/20 shadow-sm leading-none flex items-center h-6', getStatusClass(group.status)]">
                {{ getStatusLabel(group.status) }}
            </span>
           
           <!-- Action Buttons for Status -->
           <button v-if="group.status === 'Open' && can('DoanKham.Edit')" @click="$emit('update-status', group.groupId, 'Finished')" class="text-[9px] font-black bg-white text-primary hover:bg-slate-50 px-4 py-2 rounded-xl transition-all shadow-sm">KẾT THÚC ĐOÀN</button>
           
           <button v-if="group.status === 'Finished' && can('DoanKham.Lock') && lockStatus?.isReadyToLock" 
                   @click="$emit('lock-group', group.groupId)" 
                   class="text-[9px] font-black bg-emerald-500 text-white hover:bg-emerald-600 px-4 py-2 rounded-xl transition-all shadow-sm flex items-center gap-2">
               <Lock class="w-3 h-3" /> KHÓA SỔ TÀI CHÍNH
           </button>

           <div v-if="group.status === 'Locked'" class="flex items-center gap-2 px-3 py-1.5 bg-white/20 rounded-xl border border-white/10 shadow-inner">
               <CheckCircle2 class="w-3 h-3 text-white" />
               <span class="text-[9px] font-black uppercase tracking-widest text-white">Dữ liệu đã chốt sổ</span>
           </div>
        </div>
    </div>

    <div class="p-8">
        <!-- Internal Navigation Tabs -->
        <div class="flex gap-4 mb-6 border-b border-slate-100 pb-2 mt-2">
            <button @click="activeTab = 'staffs'" :class="['px-4 py-2 text-[10px] font-black uppercase tracking-widest rounded-t-xl transition-all', activeTab === 'staffs' ? 'bg-primary text-white shadow-md shadow-primary/20' : 'bg-white text-slate-400 hover:bg-slate-100']">
                <Users class="w-3 h-3 inline mr-1" /> NHÂN SỰ ĐI ĐOÀN
            </button>
            <button @click="activeTab = 'patients'" :class="['px-4 py-2 text-[10px] font-black uppercase tracking-widest rounded-t-xl transition-all', activeTab === 'patients' ? 'bg-primary text-white shadow-md shadow-primary/20' : 'bg-white text-slate-400 hover:bg-slate-100']">
                <ShieldCheck class="w-3 h-3 inline mr-1" /> BỆNH NHÂN
            </button>
            <button @click="handleSuppliesTab" :class="['px-4 py-2 text-[10px] font-black uppercase tracking-widest rounded-t-xl transition-all flex items-center gap-2', activeTab === 'supplies' ? 'bg-emerald-600 text-white shadow-md shadow-emerald-100' : 'bg-white text-slate-400 hover:bg-slate-100']">
                <Package class="w-3 h-3" /> VẬT TƯ & HAO PHÍ
            </button>
            <div class="flex-grow"></div>
            <button @click="$router.push('/reception')" class="px-4 py-2 text-[9px] font-black uppercase tracking-widest text-indigo-500 hover:bg-indigo-50 rounded-xl transition-all flex items-center gap-2">
                <ScanLine class="w-3 h-3" /> CỔNG BÁO DANH
            </button>
        </div>

        <!-- Tab Content -->
        <div class="tab-content">
            <slot :name="activeTab"></slot>
        </div>

        <!-- Financial Summary for Locked Groups -->
        <div v-if="group.status === 'Locked'" class="mt-8 p-6 bg-slate-900 rounded-3xl border border-white/5 relative overflow-hidden group/finance">
            <div class="absolute -right-4 -bottom-4 opacity-10 transform group-hover/finance:scale-110 transition-transform">
                <Scale class="w-32 h-32 text-white" />
            </div>
            <div class="relative z-10 flex flex-col md:flex-row justify-between items-start md:items-center gap-6">
                <div class="space-y-1">
                    <h5 class="text-[10px] font-black text-slate-400 uppercase tracking-widest">Giá trị quyết toán nhân sự</h5>
                    <div class="text-2xl font-black text-white italic">
                        {{ formatPrice(group.totalLaborCost || 0) }}
                    </div>
                </div>
                <div class="flex items-center gap-6">
                    <div class="text-right">
                        <p class="text-[9px] font-black text-slate-500 uppercase">Thực khám</p>
                        <p class="text-lg font-black text-white italic">{{ lockStatus?.completedPatients || 0 }} ca</p>
                    </div>
                    <div class="w-px h-10 bg-white/10"></div>
                    <div class="text-right">
                        <p class="text-[9px] font-black text-slate-500 uppercase">Nhân sự đi đoàn</p>
                        <p class="text-lg font-black text-white italic">{{ staffCount }} người</p>
                    </div>
                </div>
                <div class="bg-emerald-500/10 border border-emerald-500/20 p-4 rounded-2xl">
                    <div class="flex items-center gap-3 text-emerald-400">
                        <CheckCircle2 class="w-5 h-5" />
                        <div>
                            <p class="text-[9px] font-black uppercase tracking-widest">Trạng thái khóa</p>
                            <p class="text-[11px] font-black text-white">ĐÃ XÁC THỰC LƯƠNG</p>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Warning for Ready to Lock -->
        <div v-if="group.status === 'Finished' && lockStatus" class="mt-8 p-6 bg-slate-50 rounded-[2.5rem] border-2 border-slate-200/50 flex flex-col md:flex-row justify-between items-center gap-8 relative overflow-hidden group/lockbox">
            <div class="absolute inset-0 bg-gradient-to-br from-indigo-50/50 to-emerald-50/50 opacity-0 group-hover/lockbox:opacity-100 transition-opacity"></div>
            
            <div class="flex items-center gap-6 relative z-10">
                <div class="w-16 h-16 bg-white rounded-2xl flex items-center justify-center shadow-xl border border-slate-100">
                     <div class="relative">
                        <Stethoscope class="w-8 h-8 text-primary" />
                        <Lock v-if="!lockStatus.isReadyToLock" class="absolute -right-2 -bottom-2 w-5 h-5 text-amber-500 fill-amber-500" />
                        <CheckCircle2 v-else class="absolute -right-2 -bottom-2 w-5 h-5 text-emerald-500 fill-emerald-500" />
                     </div>
                </div>
                <div>
                    <p class="text-[10px] font-black uppercase tracking-widest text-slate-400 mb-1">Kiểm tra điều kiện chốt sổ</p>
                    <h4 class="text-xl font-black text-slate-800 italic leading-none mb-1">
                        {{ lockStatus.completedPatients }} / {{ lockStatus.totalPatients }} Ca khám hoàn tất
                    </h4>
                    <p class="text-[10px] font-black tracking-widest" :class="lockStatus.isReadyToLock ? 'text-emerald-500' : 'text-amber-500'">
                        {{ lockStatus.isReadyToLock ? '✓ Đủ điều kiện khóa sổ tài chính' : '⚠ ' + lockStatus.message }}
                    </p>
                </div>
            </div>

            <div class="flex flex-col items-center md:items-end gap-3 relative z-10">
                <button v-if="lockStatus.isReadyToLock" 
                        @click="$emit('lock-group', group.groupId)" 
                        class="btn-premium primary !bg-slate-900 !shadow-slate-200 !px-10 !py-4 !text-xs">
                    CHỐT KHÓA SỔ & TÍNH LƯƠNG
                </button>
                <router-link v-else :to="`/medical-records?groupId=${group.groupId}`" class="px-8 py-3 bg-white border-2 border-slate-200 text-slate-400 font-black text-[10px] uppercase tracking-widest rounded-2xl hover:border-primary hover:text-primary transition-all">
                    Xử lý ca tồn đọng
                </router-link>
            </div>
        </div>

        <!-- Footer: Attachments -->
        <div class="mt-10 pt-8 border-t border-slate-50 flex flex-col md:flex-row justify-between items-center gap-6">
            <div class="flex items-center gap-4">
                <FileText class="w-5 h-5 text-slate-400" />
                <span class="text-[10px] font-black uppercase tracking-widest text-slate-400 ">Dữ liệu kết quả & Tài liệu đoàn</span>
            </div>
            <div class="flex items-center gap-3">
                <div v-if="group.importFilePath" class="flex items-center gap-3 p-3 bg-emerald-50 rounded-xl border border-emerald-100">
                     <FileIcon class="w-4 h-4 text-emerald-500" />
                     <span class="text-[10px] font-black text-slate-600 truncate max-w-[150px]">{{ group.importFilePath.split('/').pop() }}</span>
                     <a :href="'/' + group.importFilePath" target="_blank" class="text-[9px] font-black text-emerald-600 uppercase tracking-widest underline ml-2">Tải về</a>
                </div>
                <button v-if="group.status === 'Open'" @click="$emit('trigger-import', group.groupId)" class="btn-import-premium">
                   IMPORT KẾT QUẢ
                </button>
            </div>
        </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { 
  Stethoscope, Lock, CheckCircle2, Users, ShieldCheck, 
  Package, ScanLine, Scale, FileText, FileIcon 
} from 'lucide-vue-next'

const props = defineProps({
  group: { type: Object, required: true },
  staffCount: { type: Number, default: 0 },
  lockStatus: { type: Object, default: null },
  can: { type: Function, required: true }
})

const emit = defineEmits(['update-status', 'lock-group', 'trigger-import', 'open-supplies'])

const activeTab = ref('staffs')

const formatDate = (date) => {
  if (!date) return '---'
  return new Date(date).toLocaleDateString('vi-VN')
}

const formatPrice = (price) => {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(price)
}

const getStatusLabel = (status) => {
  const map = { 'Open': 'Đang triển khai', 'Finished': 'Đã kết thúc', 'Locked': 'Đã khóa sổ' }
  return map[status] || status
}

const getStatusClass = (status) => {
  if (status === 'Open') return 'bg-white text-primary'
  if (status === 'Finished') return 'bg-emerald-100 text-emerald-600'
  if (status === 'Locked') return 'bg-slate-800 text-white'
  return 'bg-slate-100 text-slate-500'
}

const handleSuppliesTab = () => {
  activeTab.value = 'supplies'
  emit('open-supplies', props.group.groupId)
}
</script>

<style scoped>
.btn-import-premium {
  padding: 0.75rem 1.5rem;
  background: white;
  border: 1px solid #e2e8f0;
  color: #475569;
  font-size: 10px;
  font-weight: 900;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  border-radius: 12px;
  transition: all 0.2s;
}

.btn-import-premium:hover {
  background: #f8fafc;
  border-color: #cbd5e1;
}

.premium-card {
  transition: all 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275);
}

.premium-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.15);
}
</style>
