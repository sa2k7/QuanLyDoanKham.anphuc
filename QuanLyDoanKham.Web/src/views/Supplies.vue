<template>
  <div class="space-y-6 animate-fade-in pb-20">
    <!-- Header Section -->
    <div class="flex flex-col md:flex-row justify-between items-start md:items-center gap-6">
      <div>
        <h2 class="text-3xl font-black text-slate-800 flex items-center gap-3">
          <div class="w-12 h-12 bg-violet-600 text-white rounded-2xl flex items-center justify-center shadow-lg">
            <Package class="w-6 h-6" />
          </div>
          Hệ thống Vật tư & Kho
          <span class="text-slate-200 ml-2 font-black">/</span>
          <span class="text-violet-600 font-black tabular-nums">{{ String(list.length).padStart(3, '0') }}</span>
        </h2>
        <div class="flex items-center gap-4 mt-2">
            <button @click="activeTab = 'inventory'" :class="['text-[10px] font-black uppercase tracking-widest px-4 py-2 rounded-full transition-all', activeTab === 'inventory' ? 'bg-violet-600 text-white shadow-lg shadow-violet-200' : 'bg-slate-100 text-slate-400 hover:bg-slate-200']">
                <span class="tracking-[0.3em]">Tồn kho hiện tại</span>
            </button>
            <button @click="activeTab = 'vouchers'" :class="['text-[10px] font-black uppercase tracking-widest px-4 py-2 rounded-full transition-all', activeTab === 'vouchers' ? 'bg-slate-800 text-white shadow-lg shadow-slate-200' : 'bg-slate-100 text-slate-400 hover:bg-slate-200']">
                <span class="tracking-[0.3em]">Lịch sử Phiếu kho</span>
            </button>
        </div>
      </div>
      <div class="flex gap-3">
        <button v-if="authStore.role === 'Admin' || authStore.role === 'WarehouseManager'" 
                @click="openVoucherModal()" 
                class="btn-premium bg-white border border-slate-200 text-slate-600 px-6 py-3 shadow-sm hover:bg-slate-50 transition-all">
          <ClipboardList class="w-4 h-4 mr-2" />
          <span class="text-[10px] uppercase tracking-widest font-black ">LẬP PHIẾU MỚI</span>
        </button>
        <button v-if="authStore.role === 'Admin' || authStore.role === 'WarehouseManager'" 
                @click="openModal()" 
                class="btn-premium bg-slate-900 text-white px-8 py-3 shadow-lg transition-all">
          <Plus class="w-5 h-5 mr-2" />
          <span class="text-[10px] uppercase tracking-widest font-black ">THÊM VẬT TƯ</span>
        </button>
      </div>
    </div>

    <!-- Active View: INVENTORY -->
    <div v-if="activeTab === 'inventory'" class="space-y-6 animate-fade-in">
        <!-- Stats Overview -->
        <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
            <StatCard 
                title="Tổng danh mục mặt hàng"
                :value="String(list.length).padStart(3, '0')"
                :icon="Layers"
                variant="indigo"
                subtext="Danh mục kho"
            />
            <StatCard 
                title="Cảnh báo tồn kho thấp"
                :value="String(list.filter(s => s.isLowStock).length).padStart(3, '0')"
                :icon="AlertCircle"
                variant="rose"
                trend="Cần nhập"
                trendColor="rose"
            />
            <StatCard 
                title="Vật tư sắp hết hạn"
                :value="String(list.filter(s => s.isExpiringSoon).length).padStart(3, '0')"
                :icon="Clock"
                variant="amber"
                trend="Cần xử lý"
                trendColor="amber"
            />
        </div>

        <!-- Search & Table -->
        <div class="premium-card bg-white border border-slate-100 overflow-hidden mt-8">
            <div class="p-6 border-b border-slate-50 flex items-center gap-4 bg-slate-50/30">
                <div class="relative group flex-1">
                    <Search class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-300 w-4 h-4" />
                    <input v-model="searchQuery" placeholder="Tìm tên mặt hàng, phân loại, mã vật tư..." 
                        class="w-full pl-10 pr-4 py-3 rounded-2xl bg-white border border-slate-200 focus:border-violet-600/20 outline-none font-black text-xs text-slate-600 shadow-sm transition-all" />
                </div>
            </div>

            <div class="overflow-x-auto">
                <table class="w-full text-left">
                    <thead class="bg-slate-50 text-[10px] font-black uppercase tracking-widest text-slate-400">
                        <tr>
                            <th class="p-5 text-center w-20">STT</th>
                            <th class="p-5">Mục vật tư</th>
                            <th class="p-5">Phân loại</th>
                            <th class="p-5 text-right">Đơn giá</th>
                            <th class="p-5 text-center">Tồn kho hiện thời</th>
                            <th class="p-5 text-center">Tác vụ</th>
                        </tr>
                    </thead>
                    <tbody class="divide-y divide-slate-50">
                        <tr v-for="(item, index) in filteredList" :key="item.supplyId" 
                            class="text-xs hover:bg-slate-50/50 transition-all cursor-pointer group" @click="openModal(item)">
                            <td class="p-5 text-center font-black text-slate-300 group-hover:text-violet-600 tabular-nums">{{ String(index + 1).padStart(3, '0') }}</td>
                            <td class="p-5">
                                <div class="flex items-center gap-4">
                                    <div :class="['w-10 h-10 rounded-xl flex items-center justify-center font-black shadow-lg', 
                                                item.isLowStock ? 'bg-rose-50 text-rose-600' : 'bg-violet-50 text-violet-600']">
                                        {{ item.supplyName.charAt(0) }}
                                    </div>
                                    <div class="flex flex-col">
                                        <span class="font-black text-slate-800 uppercase tracking-widest group-hover:text-violet-600">{{ item.supplyName }}</span>
                                        <span class="text-[9px] font-black text-slate-400 uppercase tracking-widest mt-0.5">VT{{ String(item.supplyId).padStart(4, '0') }}</span>
                                    </div>
                                </div>
                            </td>
                            <td class="p-5">
                                <span class="px-3 py-1.5 bg-slate-100 rounded-lg font-black text-slate-500 uppercase tracking-widest text-[9px] ">{{ item.category || 'VTYT' }}</span>
                            </td>
                            <td class="p-5 text-right font-black text-slate-700 ">{{ formatCurrency(item.unitPrice) }}</td>
                            <td class="p-5 text-center border-l border-slate-50">
                                <div class="flex flex-col items-center">
                                    <span :class="['text-sm font-black ', item.isLowStock ? 'text-rose-500' : 'text-slate-900']">{{ formatNumber(item.totalStock) }}</span>
                                    <span class="text-[8px] font-black text-slate-400 uppercase tracking-widest mt-0.5">{{ item.unit || 'Đơn vị' }}</span>
                                </div>
                            </td>
                            <td class="p-5 text-center">
                                <button v-if="authStore.role === 'Admin' || authStore.role === 'WarehouseManager'" 
                                        @click.stop="openModal(item)" class="btn-action-premium variant-indigo text-slate-400" title="Sửa hàng">
                                    <Edit3 class="w-5 h-5" />
                                </button>
                            </td>
                        </tr>
                        <tr v-if="filteredList.length === 0">
                            <td colspan="6" class="py-24 text-center opacity-20">
                                <PackageSearch class="w-16 h-16 mx-auto mb-4" />
                                <p class="text-xs font-black uppercase tracking-[0.3em]">Kho dữ liệu trống</p>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
    </div>

    <!-- Active View: VOUCHERS -->
    <div v-else class="space-y-6 animate-fade-in">
        <div class="premium-card bg-white border border-slate-100 overflow-hidden">
            <div class="p-6 border-b border-slate-50 flex items-center justify-between bg-slate-50/20">
                <h3 class="text-sm font-black text-slate-800 uppercase tracking-widest ">Lịch sử giao dịch kho</h3>
                <div class="flex items-center gap-3">
                    <button @click="fetchVouchers" class="p-2 text-slate-400 hover:text-violet-600"><RefreshCw class="w-4 h-4" /></button>
                </div>
            </div>
            <div class="overflow-x-auto">
                <table class="w-full text-left">
                    <thead class="bg-slate-50 text-[10px] font-black uppercase tracking-widest text-slate-400">
                        <tr>
                            <th class="p-5 w-32">Mã phiếu</th>
                            <th class="p-5">Loại</th>
                            <th class="p-5 text-center w-40">Ngày tạo</th>
                            <th class="p-5">Đoàn khám / Đơn vị</th>
                            <th class="p-5">Người lập</th>
                            <th class="p-5">Ghi chú</th>
                            <th class="p-5 text-center">Tác vụ</th>
                        </tr>
                    </thead>
                    <tbody class="divide-y divide-slate-50 text-xs text-slate-600">
                        <tr v-for="v in vouchers" :key="v.voucherId" class="hover:bg-slate-50/50 transition-colors group">
                            <td class="p-5 font-black text-slate-900">{{ v.voucherCode }}</td>
                            <td class="p-5">
                                <span :class="['px-3 py-1.5 rounded-lg font-black uppercase tracking-widest text-[9px] whitespace-nowrap inline-flex items-center gap-2', v.type === 'IMPORT' ? 'bg-emerald-50 text-emerald-600' : 'bg-orange-50 text-orange-600']">
                                    <ArrowDownLeft v-if="v.type === 'IMPORT'" class="w-3 h-3" />
                                    <ArrowUpRight v-else class="w-3 h-3" />
                                    {{ v.type === 'IMPORT' ? 'Nhập kho' : 'Xuất kho' }}
                                </span>
                            </td>
                            <td class="p-5 text-center font-black text-slate-400">{{ new Date(v.createDate).toLocaleString('vi-VN') }}</td>
                            <td class="p-5">
                                <span class="font-black text-slate-700 uppercase tracking-widest">{{ v.groupName || 'KHO TỔNG' }}</span>
                            </td>
                            <td class="p-5 font-black text-slate-500">{{ v.createdBy }}</td>
                            <td class="p-5 text-slate-400 font-medium italic truncate max-w-[200px]">{{ v.note || '---' }}</td>
                            <td class="p-5 text-center">
                                <button v-if="authStore.role === 'Admin'" @click="deleteVoucher(v.voucherId)" class="p-2 text-rose-300 hover:text-rose-500 hover:bg-rose-50 rounded-xl transition-all opacity-0 group-hover:opacity-100">
                                    <Trash2 class="w-4 h-4" />
                                </button>
                            </td>
                        </tr>
                        <tr v-if="vouchers.length === 0">
                            <td colspan="6" class="py-24 text-center opacity-10 font-black uppercase tracking-widest ">Chưa có giao dịch phát sinh</td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
    </div>

    <!-- Modal: Thêm/Sửa mặt hàng -->
    <Teleport to="body">
      <div v-if="showModal" class="fixed inset-0 z-[200] flex items-center justify-center bg-slate-900/60 backdrop-blur-xl p-6 animate-fade-in overflow-y-auto">
          <div class="bg-white w-full max-w-xl rounded-[3.5rem] shadow-2xl animate-scale-up border-2 border-slate-900 relative overflow-hidden mt-auto mb-auto">
              <!-- Border Overlay -->
              <div class="absolute inset-0 rounded-[inherit] border-2 border-slate-900 pointer-events-none z-50"></div>
              
              <!-- Header Gradient -->
              <div class="absolute top-0 left-0 right-0 h-32 bg-gradient-to-r from-teal-400 to-teal-600 z-0"></div>

              <!-- Close Button -->
              <button @click="showModal = false" class="absolute top-6 right-6 bg-white/20 p-2 rounded-full hover:bg-white/40 transition-all text-white z-10 flex items-center justify-center">
                  <X class="w-6 h-6" />
              </button>

              <div class="mt-32 relative z-10 pt-4">
                  <div class="p-12 pt-4 pb-0">
                      <h3 class="text-2xl font-black text-slate-800 mb-10 uppercase tracking-widest flex items-center gap-4">
                    <div class="w-12 h-12 bg-white text-teal-600 rounded-2xl flex items-center justify-center shadow-lg border-2 border-teal-50">
                      <Edit3 class="w-6 h-6" />
                    </div>
                    Thành phần Vật tư
                  </h3>
              
              <form id="supplyForm" @submit.prevent="saveItem" class="space-y-8">
                  <div class="grid grid-cols-2 gap-8">
                    <div class="flex flex-col gap-2 col-span-2">
                        <label class="text-[10px] font-black uppercase text-slate-400 tracking-[0.2em] ml-2">Tên định danh mặt hàng *</label>
                        <input v-model="currentItem.supplyName" required class="bg-slate-50 border-2 border-slate-100 rounded-2xl px-5 py-4 text-sm font-black text-slate-700 outline-none transition-all focus:border-violet-300 focus:bg-white w-full" placeholder="Ví dụ: Thuốc giảm đau..." />
                    </div>
                    <div class="flex flex-col gap-2">
                        <label class="text-[10px] font-black uppercase text-slate-400 tracking-[0.2em] ml-2">Phân nhóm mục</label>
                        <select v-model="currentItem.category" class="bg-slate-50 border-2 border-slate-100 rounded-2xl px-5 py-4 text-sm font-black text-slate-700 outline-none transition-all focus:border-violet-300 focus:bg-white w-full font-black">
                          <option value="VTYT">Vật tư y tế</option>
                          <option value="Thuốc">Thuốc / Dược phẩm</option>
                          <option value="CCDC">Công cụ dụng cụ</option>
                          <option value="Khác">Phân loại khác</option>
                        </select>
                    </div>
                    <div class="flex flex-col gap-2">
                        <label class="text-[10px] font-black uppercase text-slate-400 tracking-[0.2em] ml-2">Đơn vị tính</label>
                        <input v-model="currentItem.unit" required class="bg-slate-50 border-2 border-slate-100 rounded-2xl px-5 py-4 text-sm font-black text-slate-700 outline-none transition-all focus:border-violet-300 focus:bg-white w-full font-black" placeholder="Lọ, Cái..." />
                    </div>
                    <div class="flex flex-col gap-2">
                        <label class="text-[10px] font-black uppercase text-slate-400 tracking-[0.2em] ml-2">Đơn giá niêm yết (VNĐ)</label>
                        <input :value="formatInput(currentItem.unitPrice)" 
                               @input="currentItem.unitPrice = parseInput($event.target.value)"
                               required class="bg-slate-50 border-2 border-slate-100 rounded-2xl px-5 py-4 text-sm font-black text-slate-700 outline-none transition-all focus:border-violet-300 focus:bg-white w-full text-right font-black" placeholder="0" />
                    </div>
                    <div class="flex flex-col gap-2">
                        <label class="text-[10px] font-black uppercase text-slate-400 tracking-[0.2em] ml-2">Ngưỡng tồn tối thiểu</label>
                        <input :value="formatInput(currentItem.minStockLevel)" 
                               @input="currentItem.minStockLevel = parseInput($event.target.value)"
                               required class="bg-slate-50 border-2 border-slate-100 rounded-2xl px-5 py-4 text-sm font-black text-slate-700 outline-none transition-all focus:border-violet-300 focus:bg-white w-full text-right font-black" placeholder="5" />
                    </div>
                  </div>

                  </form>
                  </div>

                  <div class="px-12 pb-12 pt-4 bg-white relative z-20">
                      <div class="flex gap-4">
                          <button v-if="currentItem.supplyId && authStore.role === 'Admin'" type="button" @click="deleteItem" class="px-6 py-4 text-rose-500 font-black uppercase tracking-widest text-[10px] hover:bg-rose-50 rounded-2xl">Xóa</button>
                          <div class="flex-1"></div>
                          <button type="button" @click="showModal = false" class="px-8 py-4 text-slate-400 font-black uppercase tracking-widest text-[10px] underline">Hủy</button>
                          <button form="supplyForm" type="submit" class="bg-slate-900 text-white px-12 py-4 rounded-2xl font-black shadow-xl shadow-slate-200 uppercase text-[10px] tracking-[0.2em]">Cập nhật hàng</button>
                      </div>
                  </div>
              </div>
          </div>
      </div>
    </Teleport>

    <!-- Modal: Lập Phiếu Kho (Nghiệp vụ quan trọng) -->
    <Teleport to="body">
      <div v-if="showVoucherModal" class="fixed inset-0 z-[200] flex items-center justify-center bg-slate-900/40 backdrop-blur-xl p-6 animate-fade-in overflow-y-auto">
          <div class="bg-white w-full max-w-4xl p-12 rounded-[3.5rem] shadow-2xl animate-scale-up border-2 border-slate-900 overflow-hidden relative mt-auto mb-auto">
              <!-- Border Overlay -->
              <div class="absolute inset-0 rounded-[inherit] border-2 border-slate-900 pointer-events-none z-50"></div>
              
              <!-- Header Gradient -->
              <div class="absolute top-0 left-0 right-0 h-32 bg-gradient-to-r from-teal-400 to-teal-600 z-0"></div>

              <!-- Close Button -->
              <button @click="showVoucherModal = false" class="absolute top-6 right-6 bg-white/20 p-2 rounded-full hover:bg-white/40 transition-all text-white z-10 flex items-center justify-center">
                  <X class="w-6 h-6" />
              </button>

              <div class="mt-32 relative z-10 pt-4">
                  <div class="px-12">
                      <div class="flex justify-between items-center mb-10">
                      <h3 class="text-2xl font-black text-slate-800 uppercase tracking-widest flex items-center gap-4">
                        <div class="w-12 h-12 bg-white text-teal-600 rounded-2xl flex items-center justify-center shadow-lg border-2 border-teal-50">
                          <ClipboardList class="w-6 h-6" />
                        </div>
                        Lập Giao dịch Kho
                      </h3>
                  <div class="flex gap-2">
                      <button @click="newVoucher.type = 'IMPORT'" :class="['px-6 py-3 rounded-xl font-black uppercase text-[10px] tracking-[0.2em] transition-all', newVoucher.type === 'IMPORT' ? 'bg-emerald-600 text-white shadow-lg' : 'bg-slate-100 text-slate-400']">Nhập kho</button>
                      <button @click="newVoucher.type = 'EXPORT'" :class="['px-6 py-3 rounded-xl font-black uppercase text-[10px] tracking-[0.2em] transition-all', newVoucher.type === 'EXPORT' ? 'bg-orange-500 text-white shadow-lg' : 'bg-slate-100 text-slate-400']">Xuất kho</button>
                  </div>
              </div>
              
              <div class="pr-2 space-y-8 pb-4">
                  <!-- Voucher Header Info -->
                  <div class="grid grid-cols-2 gap-8">
                      <div v-if="newVoucher.type === 'EXPORT'" class="flex flex-col gap-2">
                          <label class="text-[10px] font-black uppercase text-slate-400 tracking-[0.2em] ml-2">Đoàn khám tiếp nhận *</label>
                          <select v-model="newVoucher.groupId" required class="bg-slate-50 border-2 border-slate-100 rounded-2xl px-5 py-4 text-sm font-black text-slate-700 outline-none transition-all focus:border-violet-300 focus:bg-white w-full font-black">
                              <option v-for="g in groups" :key="g.groupId" :value="g.groupId">[{{ g.groupCode }}] - {{ g.groupName }}</option>
                          </select>
                      </div>
                      <div class="flex flex-col gap-2" :class="{'col-span-2': newVoucher.type === 'IMPORT'}">
                          <label class="text-[10px] font-black uppercase text-slate-400 tracking-[0.2em] ml-2">Ghi chú nội dung phiếu</label>
                          <input v-model="newVoucher.note" class="bg-slate-50 border-2 border-slate-100 rounded-2xl px-5 py-4 text-sm font-black text-slate-700 outline-none transition-all focus:border-violet-300 focus:bg-white w-full font-black" placeholder="Lý do nhập/xuất, chứng từ đính kèm..." />
                      </div>
                  </div>

                  <!-- Voucher Details -->
                  <div class="space-y-4">
                      <div class="flex items-center justify-between">
                          <p class="text-[10px] font-black uppercase tracking-widest text-slate-400 ">Danh mục hàng hóa chuyển kho</p>
                          <button @click="addDetailRow" class="text-[9px] font-black text-violet-600 uppercase tracking-widest flex items-center gap-2 hover:bg-violet-50 px-3 py-1.5 rounded-lg transition-all">
                              <PlusCircle class="w-4 h-4" /> Thêm dòng hàng
                          </button>
                      </div>

                      <div class="space-y-4">
                          <div v-for="(row, idx) in newVoucher.details" :key="idx" class="grid grid-cols-12 gap-4 items-end bg-slate-50/50 p-4 rounded-3xl border border-slate-100 animate-fade-in-up">
                              <div class="col-span-6 flex flex-col gap-2">
                                  <label class="text-[8px] font-black text-slate-400 uppercase tracking-widest ml-1">Vật tư / Thuốc</label>
                                  <select v-model="row.supplyId" class="bg-slate-50 border-2 border-slate-100 rounded-2xl px-5 py-4 text-sm font-black text-slate-700 outline-none transition-all focus:border-violet-300 focus:bg-white w-full font-black text-xs">
                                      <option v-for="s in list" :key="s.supplyId" :value="s.supplyId">{{ s.supplyName }} (Tồn: {{ s.totalStock }} {{ s.unit }})</option>
                                  </select>
                              </div>
                              <div class="col-span-4 flex flex-col gap-2">
                                  <label class="text-[8px] font-black text-slate-400 uppercase tracking-widest ml-1">Số lượng {{ newVoucher.type === 'IMPORT' ? 'nhập' : 'xuất' }}</label>
                                  <input :value="formatInput(row.quantity)" 
                                         @input="row.quantity = parseInput($event.target.value)"
                                         class="bg-slate-50 border-2 border-slate-100 rounded-2xl px-5 py-4 text-sm font-black text-slate-700 outline-none transition-all focus:border-violet-300 focus:bg-white w-full font-black text-right" placeholder="0" />
                              </div>
                              <div class="col-span-2 flex justify-center pb-1">
                                  <button @click="removeDetailRow(idx)" class="p-4 text-rose-300 hover:text-rose-500 hover:bg-rose-50 rounded-2xl transition-all"><Trash2 class="w-5 h-5" /></button>
                              </div>
                          </div>
                      </div>
                  </div>
              </div>

                  </div>

                  <div class="px-12 pb-12 pt-4 bg-white relative z-20">
                      <div class="flex gap-4">
                          <button type="button" @click="showVoucherModal = false" class="px-8 py-4 text-slate-400 font-black uppercase tracking-widest text-[10px] underline">Hủy bỏ</button>
                          <div class="flex-1"></div>
                          <button @click="saveVoucher" :disabled="!isVoucherValid" class="bg-slate-900 text-white px-14 py-4 rounded-2xl font-black shadow-2xl flex items-center gap-4 uppercase text-[10px] tracking-[0.3em] disabled:opacity-30 disabled:scale-100 transform hover:-translate-y-1 active:scale-95 transition-all">
                              Xác nhận chốt phiếu
                          </button>
                      </div>
                  </div>
              </div>
          </div>
      </div>
    </Teleport>
  </div>
</template>

<script setup>
import { ref, onMounted, computed, watch } from 'vue'
import axios from 'axios'
import { 
    Package, Plus, Search, RefreshCw, Layers, AlertCircle, Clock, Edit3, 
    PackageSearch, ClipboardList, PlusCircle, Trash2, ChevronRight,
    ArrowDownLeft, ArrowUpRight, X
} from 'lucide-vue-next'
import { useAuthStore } from '../stores/auth'
import { useToast } from '../composables/useToast'
import StatCard from '../components/StatCard.vue'

const authStore = useAuthStore()
const toast = useToast()

// -- STATE --
const activeTab = ref('inventory')
const list = ref([])
const vouchers = ref([])
const groups = ref([])
const showModal = ref(false)
const showVoucherModal = ref(false)
const searchQuery = ref('')
const currentItem = ref({})

const newVoucher = ref({
    type: 'IMPORT',
    groupId: null,
    note: '',
    details: [{ supplyId: null, quantity: 0 }]
})

// -- UTILS: Number & Currency Formatting --
const formatNumber = (num) => new Intl.NumberFormat('vi-VN').format(num || 0)
const formatCurrency = (val) => new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(val || 0)

const formatInput = (val) => {
    if (val === null || val === undefined || isNaN(val)) return ''
    return new Intl.NumberFormat('vi-VN').format(val)
}

const parseInput = (str) => {
    if (!str) return 0
    const clean = str.replace(/[^\d]/g, '')
    return parseInt(clean) || 0
}

// -- COMPUTED --
const filteredList = computed(() => {
    if (!searchQuery.value) return list.value
    const q = searchQuery.value.toLowerCase()
    return list.value.filter(s => 
        s.supplyName.toLowerCase().includes(q) || 
        (s.category && s.category.toLowerCase().includes(q)) ||
        String(s.supplyId).includes(q)
    )
})

const isVoucherValid = computed(() => {
    if (newVoucher.value.type === 'EXPORT' && !newVoucher.value.groupId) return false
    return newVoucher.value.details.some(d => d.supplyId && d.quantity > 0)
})

// -- DATA FETCHING --
const fetchList = async () => {
    try {
        const res = await axios.get('http://localhost:5283/api/Supplies')
        list.value = res.data
    } catch (e) { toast.error("Lỗi dữ liệu kho") }
}

const fetchVouchers = async () => {
    try {
        const res = await axios.get('http://localhost:5283/api/Supplies/vouchers')
        vouchers.value = res.data
    } catch (e) { toast.error("Lỗi dữ liệu phiếu") }
}

const fetchGroups = async () => {
    try {
        const res = await axios.get('http://localhost:5283/api/MedicalGroups')
        // Lọc bỏ các đoàn khám đã hoàn tất (Completed) để không hiển thị trong danh sách chọn
        groups.value = res.data.filter(g => {
            const status = g.status || g.Status
            return status !== 'Completed'
        })
    } catch (e) { console.warn("Could not fetch groups") }
}

// -- ACTIONS: SUPPLIES --
const openModal = (item = null) => {
    currentItem.value = item ? { ...item } : { supplyName: '', category: 'VTYT', unit: '', unitPrice: 0, minStockLevel: 5 }
    showModal.value = true
}

const saveItem = async () => {
    try {
        if (currentItem.value.supplyId) {
            await axios.put(`http://localhost:5283/api/Supplies/${currentItem.value.supplyId}`, currentItem.value)
        } else {
            await axios.post('http://localhost:5283/api/Supplies', currentItem.value)
        }
        toast.success("Hợp lệ: Dữ liệu đã được cập nhật!")
        showModal.value = false
        fetchList()
    } catch (e) { 
        toast.error(e.response?.data || "Lỗi lưu dữ liệu") 
    }
}

const deleteItem = async () => {
    if (!confirm("Xác nhận xóa mặt hàng này khỏi kho?")) return
    try {
        await axios.delete(`http://localhost:5283/api/Supplies/${currentItem.value.supplyId}`)
        toast.success("Đã xóa khỏi danh mục!")
        showModal.value = false
        fetchList()
    } catch (e) { 
        toast.error(e.response?.data || "Không thể xóa hàng đang lưu kho") 
    }
}

// -- ACTIONS: VOUCHERS --
const openVoucherModal = () => {
    newVoucher.value = {
        type: 'IMPORT',
        groupId: null,
        note: '',
        details: [{ supplyId: list.value[0]?.supplyId || null, quantity: 10 }]
    }
    showVoucherModal.value = true
}

const addDetailRow = () => {
    newVoucher.value.details.push({ supplyId: list.value[0]?.supplyId || null, quantity: 0 })
}

const removeDetailRow = (idx) => {
    if (newVoucher.value.details.length > 1) {
        newVoucher.value.details.splice(idx, 1)
    }
}

const saveVoucher = async () => {
    try {
        await axios.post('http://localhost:5283/api/Supplies/vouchers', newVoucher.value)
        toast.success("Phiếu kho đã được chốt và cập nhật tồn kho!")
        showVoucherModal.value = false
        fetchList()
        fetchVouchers()
    } catch (e) {
        toast.error(e.response?.data || "Lỗi lập phiếu")
    }
}

const deleteVoucher = async (id) => {
    if (!confirm("Xác nhận xóa phiếu kho này? Tồn kho liên quan sẽ được tự động hoàn tác.")) return
    try {
        await axios.delete(`http://localhost:5283/api/Supplies/vouchers/${id}`)
        toast.success("Đã xóa phiếu và hoàn tác tồn kho!")
        fetchVouchers()
        fetchList()
    } catch (e) {
        toast.error(e.response?.data || "Lỗi xóa phiếu")
    }
}

// -- LIFECYCLE --
onMounted(() => {
    fetchList()
    fetchVouchers()
    fetchGroups()
})

// Watch for tab switching to refresh data
watch(activeTab, (val) => {
    if (val === 'vouchers') fetchVouchers()
    else fetchList()
})
</script>

<style scoped>
.animate-scale-up {
  animation: scaleUp 0.6s cubic-bezier(0.16, 1, 0.3, 1) forwards;
}
@keyframes scaleUp {
  from { opacity: 0; transform: scale(0.92) translateY(20px); }
  to { opacity: 1; transform: scale(1) translateY(0); }
}

.animate-fade-in {
  animation: fadeIn 0.5s ease-out forwards;
}
@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}

.animate-fade-in-up {
  animation: fadeInUp 0.4s ease-out forwards;
}
@keyframes fadeInUp {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}

.premium-card {
    transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1);
}
.premium-card:hover {
    transform: translateY(-4px);
    box-shadow: 0 20px 40px -15px rgba(0,0,0,0.05);
}
</style>
