<template>
  <div class="min-h-screen flex dashboard-gradient font-sans overflow-hidden">
    <!-- Mobile Backdrop -->
    <div v-if="isMobileMenuOpen" @click="isMobileMenuOpen = false" class="fixed inset-0 bg-slate-900/50 backdrop-blur-sm z-[55] md:hidden"></div>

    <!-- Sidebar Navigation -->
    <aside :class="['sidebar-gradient border-r border-slate-100 flex flex-col h-screen fixed md:sticky top-0 z-[60] shadow-[20px_0_40px_-20px_rgba(0,0,0,0.03)] flex-shrink-0 transition-all duration-300 ease-in-out', 
                    isSidebarCollapsed ? 'w-24' : 'w-60',
                    isMobileMenuOpen ? 'translate-x-0' : '-translate-x-full md:translate-x-0']">
      <!-- Collapse Toggle Button (Desktop) -->
      <button @click="isSidebarCollapsed = !isSidebarCollapsed" 
              class="hidden md:flex absolute -right-4 top-10 w-8 h-8 bg-white border border-slate-100 rounded-full items-center justify-center shadow-lg text-slate-400 hover:text-primary z-[70] transition-all"
              :class="{'rotate-180': isSidebarCollapsed}">
        <ChevronLeft class="w-4 h-4" />
      </button>

      <!-- Logo Section -->
      <div class="p-6 pb-8 flex items-center gap-3 cursor-pointer group overflow-hidden" @click="activeMenu = 'home'">
        <div class="bg-white p-1 rounded-2xl transition-all group-hover:rotate-6 shadow-lg shadow-primary/20 flex-shrink-0 border border-slate-100">
          <img :src="logo" class="w-10 h-10 object-contain" alt="Logo" />
        </div>
        <div v-show="!isSidebarCollapsed" class="transition-opacity duration-300">
          <h1 class="font-bold text-lg text-slate-900 leading-tight tracking-tight">ĐA KHOA <span class="text-primary italic">AN PHÚC</span></h1>
          <p class="text-[8px] font-semibold text-slate-400 uppercase tracking-[0.3em] mt-1">Hệ thống Điều hành</p>
        </div>
      </div>

      <!-- Menu Items -->
      <nav class="flex-1 px-4 space-y-2 overflow-y-auto custom-scrollbar">
        <button v-for="item in filteredMenuItems" :key="item.id"
                @click="activeMenu = item.id; isMobileMenuOpen = false"
                :class="['w-full flex items-center flex-nowrap gap-4 px-5 py-4 rounded-2xl font-bold text-sm transition-all group relative overflow-hidden', 
                         activeMenu === item.id ? 'bg-primary text-white shadow-xl shadow-primary/20' : 'text-slate-400 hover:bg-slate-50 hover:text-slate-600',
                         isSidebarCollapsed ? 'justify-center px-0' : '']">
          <component :is="item.icon" :class="['w-5 h-5 flex-shrink-0 transition-transform group-hover:scale-110', activeMenu === item.id ? 'text-white' : 'text-slate-300 group-hover:text-primary']" />
          <span v-show="!isSidebarCollapsed" class="tracking-[0.2em] uppercase whitespace-nowrap flex-shrink-0 transition-opacity duration-300">{{ item.name }}</span>
          
          <div v-if="activeMenu === item.id" class="absolute right-0 top-0 bottom-0 w-1.5 bg-white/20"></div>
          <ArrowRight v-if="activeMenu !== item.id && !isSidebarCollapsed" class="w-4 h-4 ml-auto opacity-0 group-hover:opacity-100 transition-all translate-x-2 group-hover:translate-x-0" />
        </button>
      </nav>

      <!-- User Profile Card -->
      <div :class="['border-t border-slate-100 bg-slate-50/50 transition-all duration-300', isSidebarCollapsed ? 'p-4' : 'p-6']">
        <div :class="['bg-white rounded-2xl shadow-sm border border-slate-100 group relative transition-all duration-300', isSidebarCollapsed ? 'p-3 flex justify-center' : 'p-4']">
          <div class="flex items-center gap-3">
            <div class="w-10 h-10 rounded-xl bg-primary/10 flex items-center justify-center text-primary flex-shrink-0">
              <User class="w-5 h-5" />
            </div>
            <div v-show="!isSidebarCollapsed" class="flex-1 min-w-0 transition-opacity duration-300">
              <p class="text-xs font-black text-slate-800 truncate">{{ authStore.user?.username }}</p>
              <p class="text-[9px] font-black text-primary uppercase tracking-[0.3em]">{{ i18n.t('roles.' + authStore.userRole) }}</p>
            </div>
            <button v-show="!isSidebarCollapsed" @click="isUserMenuOpen = !isUserMenuOpen" class="p-2 hover:bg-slate-50 rounded-lg transition-all">
              <ChevronDown class="w-4 h-4 text-slate-400" :class="{'rotate-180': isUserMenuOpen}" />
            </button>
          </div>

          <!-- Mini Dropdown -->
          <div v-if="isUserMenuOpen" class="absolute bottom-full left-0 right-0 mb-2 bg-white rounded-2xl shadow-2xl border border-slate-100 p-2 animate-slide-up z-[70]">
            <button @click="showPasswordModal = true; isUserMenuOpen = false" class="w-full flex items-center gap-3 px-4 py-3 rounded-xl hover:bg-slate-50 text-xs font-black text-slate-600 transition-all">
              <KeyRound class="w-4 h-4" /> <span class="">ĐỔI MẬT KHẨU</span>
            </button>
            <button @click="handleLogout" class="w-full flex items-center gap-3 px-4 py-3 rounded-xl hover:bg-rose-50 text-xs font-black text-rose-500 transition-all">
              <LogOut class="w-4 h-4" /> <span class="">ĐĂNG XUẤT</span>
            </button>
          </div>
        </div>
      </div>
    </aside>

    <!-- Main Content Area -->
    <main class="flex-1 h-screen overflow-y-auto relative custom-scrollbar w-full">
      <!-- Top Header / Global Search -->
      <header class="h-24 glass-header shadow-sm flex items-center px-4 md:px-10 sticky top-0 z-50">
        <div class="flex-1 flex items-center gap-4 md:gap-8 overflow-hidden">
          <button @click="isMobileMenuOpen = true" class="p-2 -ml-2 text-slate-400 hover:text-primary transition-colors md:hidden focus:outline-none">
            <Menu class="w-6 h-6" />
          </button>
          <div class="items-center gap-2 hidden sm:flex">
            <h2 class="text-xl font-black text-slate-800 uppercase tracking-widest whitespace-nowrap ">{{ activeMenuName }}</h2>
            <div class="w-1 h-1 rounded-full bg-slate-300"></div>
            <p class="text-[10px] font-black text-slate-400 uppercase tracking-[0.3em] whitespace-nowrap">{{ itemsBreadcrumb }}</p>
          </div>

          <!-- Global Search -->
          <div class="relative w-full max-w-xl group">
            <Search class="absolute left-6 top-1/2 -translate-y-1/2 w-5 h-5 text-slate-300 group-focus-within:text-primary transition-colors" />
            <input type="text" 
                   v-model="searchQuery"
                   @input="handleSearch"
                   @focus="isSearchFocused = true"
                   :placeholder="i18n.t('common.searchPlaceholder')"
                   class="w-full pl-14 pr-20 py-4 bg-slate-50 border border-slate-200 focus:bg-white focus:border-primary/30 focus:shadow-[0_0_0_4px_rgba(16,185,129,0.1)] rounded-2xl outline-none font-black text-slate-800 transition-all shadow-inner" />
            <div class="absolute right-6 top-1/2 -translate-y-1/2 flex gap-1">
              <span class="px-2 py-1 bg-white border border-slate-200 rounded-md text-[9px] font-black text-slate-400 shadow-sm">CTRL</span>
              <span class="px-2 py-1 bg-white border border-slate-200 rounded-md text-[9px] font-black text-slate-400 shadow-sm">K</span>
            </div>

            <!-- Enhanced Search Results -->
            <div v-if="searchResults.length > 0 && isSearchFocused" class="absolute top-full left-0 right-0 mt-4 bg-white rounded-[2rem] shadow-2xl border-2 border-slate-50 overflow-hidden z-[100] animate-scale-up">
              <div v-for="res in searchResults" :key="res.id" 
                   @click="navigateSearchResult(res)"
                   class="flex items-center p-5 hover:bg-indigo-50/50 cursor-pointer border-b border-slate-50 last:border-none transition-all group">
                <div :class="['w-12 h-12 rounded-2xl flex items-center justify-center mr-5 shadow-sm group-hover:scale-110 transition-transform duration-500', 
                     res.type === 'company' ? 'bg-sky-50 text-sky-600' : 
                     res.type === 'contract' ? 'bg-teal-50 text-teal-600' :
                     res.type === 'staff' ? 'bg-rose-50 text-rose-600' :
                     res.type === 'group' ? 'bg-primary/10 text-primary' :
                     'bg-violet-50 text-violet-600']">
                  <component :is="getSearchIcon(res.type)" class="w-6 h-6" />
                </div>
                <div class="flex-1">
                  <p class="font-black text-slate-800 leading-tight">{{ res.name }}</p>
                  <div class="flex items-center gap-2 mt-0.5">
                    <span class="text-[9px] font-black uppercase tracking-widest text-slate-400 ">{{ getResTypeName(res.type) }}</span>
                    <div class="w-1 h-1 rounded-full bg-slate-200"></div>
                    <span class="text-[9px] font-black text-slate-400">ID: #{{ res.id }}</span>
                  </div>
                </div>
                <div class="w-10 h-10 rounded-full border border-slate-100 flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity translate-x-4 group-hover:translate-x-0">
                  <ArrowRight class="w-4 h-4 text-primary" />
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Language Switcher & Notifications -->
        <div class="flex items-center gap-4">
          <!-- Language Selector -->
          <div class="flex bg-slate-100 p-1 rounded-xl">
            <button @click="i18n.setLocale('vi')" 
                    :class="['px-3 py-1.5 rounded-lg text-[10px] font-black transition-all', i18n.locale === 'vi' ? 'bg-white shadow-sm text-primary' : 'text-slate-400']">
              VN
            </button>
            <button @click="i18n.setLocale('en')" 
                    :class="['px-3 py-1.5 rounded-lg text-[10px] font-black transition-all', i18n.locale === 'en' ? 'bg-white shadow-sm text-primary' : 'text-slate-400']">
              EN
            </button>
          </div>

          <div class="relative notification-area">
            <button @click="showNotificationDropdown = !showNotificationDropdown"
                    class="p-4 bg-slate-50 text-slate-400 rounded-2xl hover:text-primary transition-all relative">
              <Bell class="w-6 h-6" />
              <div v-if="notificationStore.unreadCount > 0" class="absolute top-3 right-3 text-[8px] flex items-center justify-center min-w-[20px] h-5 px-1.5 bg-rose-500 text-white rounded-full border-2 border-white font-black tabular-nums">
                {{ notificationStore.unreadCount > 99 ? '99+' : notificationStore.unreadCount }}
              </div>
            </button>

            <!-- Notification Dropdown -->
            <div v-if="showNotificationDropdown" class="absolute top-full right-0 mt-4 w-96 bg-white rounded-[2rem] shadow-2xl border border-slate-100 overflow-hidden z-[100] animate-scale-up">
              <div class="p-6 border-b border-slate-50 flex justify-between items-center bg-slate-50/50">
                <h3 class="font-black text-slate-800 text-sm uppercase tracking-[0.2em]">Thông báo nội bộ</h3>
                <button v-if="notificationStore.unreadCount > 0" @click="notificationStore.markAllAsRead()" class="text-[10px] font-black text-primary hover:underline uppercase tracking-widest ">Đã đọc tất cả</button>
              </div>
              
              <div class="max-h-[28rem] overflow-y-auto custom-scrollbar">
                <div v-for="n in notificationStore.notifications" :key="n.id" 
                     :class="['p-5 border-b border-slate-50 last:border-none transition-all flex gap-4 group cursor-default', !n.isRead ? 'bg-indigo-50/30' : 'hover:bg-slate-50/50']">
                  <div :class="['w-10 h-10 rounded-xl flex-shrink-0 flex items-center justify-center', !n.isRead ? 'bg-primary text-white shadow-lg shadow-primary/20' : 'bg-slate-100 text-slate-300']">
                    <Sparkles class="w-5 h-5" />
                  </div>
                  <div class="flex-1">
                    <p :class="['text-xs leading-relaxed', !n.isRead ? 'text-slate-900 font-black' : 'text-slate-500']">{{ n.message }}</p>
                    <div class="flex items-center justify-between mt-2">
                      <span class="text-[9px] font-black text-slate-400 uppercase tracking-[0.3em]">{{ new Date(n.createdAt).toLocaleString('vi-VN') }}</span>
                      <div class="flex gap-2 opacity-0 group-hover:opacity-100 transition-opacity">
                        <button v-if="!n.isRead" @click="notificationStore.markAsRead(n.id)" class="p-1 hover:text-primary"><Check class="w-3 h-3" /></button>
                        <button @click="notificationStore.deleteNotification(n.id)" class="p-1 hover:text-rose-500"><X class="w-3 h-3" /></button>
                      </div>
                    </div>
                  </div>
                </div>
                
                <div v-if="notificationStore.notifications.length === 0" class="py-16 text-center">
                  <div class="w-12 h-12 bg-slate-50 rounded-full flex items-center justify-center mx-auto mb-3">
                    <Bell class="w-6 h-6 text-slate-200" />
                  </div>
                  <p class="text-[10px] font-black uppercase tracking-widest text-slate-300">Không có thông báo mới</p>
                </div>
              </div>

              <div v-if="authStore.isAdmin && resetRequests.length > 0" class="p-4 bg-rose-50/50 border-t border-rose-100">
                  <button @click="showResetModal = true; showNotificationDropdown = false" class="w-full py-4 bg-rose-500 text-white rounded-xl text-[10px] font-black uppercase tracking-[0.3em] shadow-lg shadow-rose-200 flex items-center justify-center gap-2">
                    <ShieldAlert class="w-4 h-4" />
                    {{ resetRequests.length }} YÊU CẦU CẤP LẠI MK
                  </button>
              </div>
            </div>
          </div>

          <div class="w-px h-10 bg-slate-100"></div>
          <button @click="activeMenu = 'reports'" class="flex items-center gap-3 px-6 py-3 bg-slate-900 text-white rounded-2xl font-black text-sm uppercase tracking-widest shadow-lg shadow-slate-200 active:scale-95 transition-all">
             <BarChart3 class="w-5 h-5 text-indigo-400" />
             {{ i18n.locale === 'vi' ? 'Thống kê' : 'Reports' }}
          </button>
        </div>
      </header>

      <!-- Password Reset Requests Modal (UI Update) -->
      <Teleport to="body">
        <div v-if="showResetModal" class="fixed inset-0 z-[1000] flex items-center justify-center p-6 bg-slate-900/40 backdrop-blur-md animate-fade-in">
            <div class="bg-white w-full max-w-lg rounded-[2.5rem] shadow-2xl p-10 border-2 border-indigo-50 animate-scale-up">
                <div class="flex justify-between items-center mb-8">
                    <div class="flex items-center gap-4">
                        <div class="w-12 h-12 bg-rose-50 text-rose-500 rounded-2xl flex items-center justify-center">
                            <ShieldAlert class="w-6 h-6" />
                        </div>
                        <div>
                          <h3 class="text-xl font-black text-slate-800">Yêu cầu cấp lại mật khẩu</h3>
                          <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest ">{{ resetRequests.length }} yêu cầu chưa xử lý</p>
                        </div>
                    </div>
                    <button @click="showResetModal = false" class="p-2 hover:bg-slate-50 rounded-xl transition-all"><X class="w-6 h-6 text-slate-300" /></button>
                </div>

                <div class="space-y-4 max-h-80 overflow-y-auto custom-scrollbar pr-2">
                    <div v-for="req in resetRequests" :key="req.id" class="p-6 bg-slate-50 rounded-2xl border-2 border-transparent hover:border-indigo-100 transition-all flex justify-between items-center group">
                        <div>
                            <p class="font-black text-slate-800">@{{ req.username }}</p>
                            <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mt-1">{{ new Date(req.requestedDate).toLocaleString('vi-VN') }}</p>
                        </div>
                        <button @click="handleProcessReset(req.id)" class="px-5 py-2.5 bg-slate-900 text-white rounded-xl text-[10px] font-black uppercase tracking-widest shadow-lg shadow-slate-200 hover:bg-indigo-600 transition-all">
                          Cấp lại MK
                        </button>
                    </div>

                    <div v-if="resetRequests.length === 0" class="py-20 text-center">
                        <div class="w-16 h-16 bg-slate-50 rounded-full flex items-center justify-center mx-auto mb-4">
                            <CheckCircle2 class="w-8 h-8 text-slate-200" />
                        </div>
                        <p class="text-xs font-black uppercase tracking-widest text-slate-300">Không có yêu cầu chờ xử lý</p>
                    </div>
                </div>

                    <p class="text-[9px] font-black text-slate-400 text-center uppercase tracking-widest ">Mật khẩu mặc định sau khi cấp lại: <span class="text-indigo-600 font-black">HealthCare2026</span></p>
            </div>
        </div>
      </Teleport>




      <!-- View Container -->
      <div class="p-10">
          <!-- ══ HOME: Role-Based Panel ══════════════════════════════════════ -->
          <div v-if="activeMenu === 'home'" class="animate-fade-in">
            <component
              :is="currentHomePanel"
              :today-assignment="todayAssignment"
              :is-checking-in="isCheckingIn"
              @check-in="handleCheckInConfirm"
              @navigate="(id) => { activeMenu = id; isMobileMenuOpen = false }"
            />
          </div>

          <!-- ══ OTHER MODULES ══════════════════════════════════════════════════ -->
          <div v-else class="animate-fade-in-up">
              <AnalyticsDashboard v-if="activeMenu === 'analytics' || activeMenu === 'reports'" />
              <Companies v-if="activeMenu === 'companies'" />
              <Contracts v-if="activeMenu === 'contracts'" />
              <Staff v-if="activeMenu === 'staff'" />
              <Groups v-if="activeMenu === 'groups'" />
              <AccountManager v-if="activeMenu === 'users'" />
              <Permissions v-if="activeMenu === 'permissions'" />
              <SettlementReport v-if="activeMenu === 'settlement-report'" />
              <Patients v-if="activeMenu === 'patients'" />
              <Payroll v-if="activeMenu === 'payroll'" />
              <Supplies v-if="activeMenu === 'supplies'" />
              
              <div v-if="!['companies', 'contracts', 'staff', 'groups', 'patients', 'users', 'permissions', 'analytics', 'reports', 'settlement-report', 'payroll', 'supplies'].includes(activeMenu)" 
                   class="flex flex-col items-center justify-center py-40 bg-white rounded-[4rem] border-4 border-dashed border-slate-50">
                <div class="w-24 h-24 bg-slate-50 rounded-full flex items-center justify-center mb-6">
                    <component :is="activeIcon" class="w-12 h-12 text-slate-100" />
                </div>
                <p class="text-xl font-black text-slate-300 uppercase tracking-widest ">Module {{ activeMenuName }} đang sẵn sàng</p>
                <button @click="activeMenu = 'home'" class="mt-8 px-8 py-4 bg-primary text-white rounded-[2rem] font-black uppercase tracking-widest shadow-lg shadow-primary/20 active:scale-95 transition-all">Quay lại trang chủ</button>
              </div>
          </div>
      </div>
    </main>
  </div>



    <!-- Change Password Modal (Synchronized Styled) -->
    <Teleport to="body">
      <div v-if="showPasswordModal" class="fixed inset-0 z-[1001] flex items-center justify-center p-6 bg-slate-900/60 backdrop-blur-xl animate-fade-in">
          <div class="bg-white w-full max-w-md rounded-[2.5rem] shadow-2xl p-10 border-2 border-slate-50 animate-scale-up">
              <div class="flex justify-between items-center mb-10">
                  <div class="flex items-center gap-4">
                      <div class="w-12 h-12 bg-indigo-50 text-indigo-600 rounded-2xl flex items-center justify-center">
                          <KeyRound class="w-6 h-6" />
                      </div>
                      <h3 class="text-xl font-black text-slate-800">Cập nhật mật khẩu</h3>
                  </div>
                  <button @click="showPasswordModal = false" class="text-slate-300 hover:bg-slate-50 p-2 rounded-xl transition-all"><X class="w-6 h-6" /></button>
              </div>
              
              <form @submit.prevent="handleChangePassword" class="space-y-6">
                  <div class="flex flex-col gap-2">
                      <label class="text-[10px] font-black uppercase tracking-[0.2em] text-slate-400 ml-1">Mật khẩu hiện tại</label>
                      <input v-model="passForm.currentPassword" 
                             @input="passForm.currentPassword = $event.target.value.replace(/[^\x00-\x7F]/g, '')"
                             type="password" required class="input-premium w-full" placeholder="••••••••" />
                  </div>
                  <div class="flex flex-col gap-2">
                      <label class="text-[10px] font-black uppercase tracking-[0.2em] text-slate-400 ml-1">Mật khẩu mới</label>
                      <input v-model="passForm.newPassword" 
                             @input="passForm.newPassword = $event.target.value.replace(/[^\x00-\x7F]/g, '')"
                             type="password" required class="input-premium w-full" placeholder="Tối thiểu 6 ký tự" />
                  </div>
                  <div class="flex flex-col gap-2">
                      <label class="text-[10px] font-black uppercase tracking-[0.2em] text-slate-400 ml-1">Xác nhận mật khẩu mới</label>
                      <input v-model="passForm.confirmPassword" 
                             @input="passForm.confirmPassword = $event.target.value.replace(/[^\x00-\x7F]/g, '')"
                             type="password" required class="input-premium w-full" placeholder="Nhập lại mật khẩu mới" />
                  </div>
                  
                  <div class="pt-6 flex gap-4">
                      <button type="button" @click="showPasswordModal = false" class="flex-1 py-4 text-slate-400 font-black text-xs uppercase tracking-widest ">Hủy bỏ</button>
                      <button type="submit" :disabled="isChangingPass" class="flex-1 bg-primary text-white py-4 rounded-2xl font-black text-xs uppercase tracking-widest shadow-xl shadow-primary/20 active:scale-95 disabled:opacity-50">
                          {{ isChangingPass ? 'Đang gửi...' : 'CẬP NHẬT NGAY' }}
                      </button>
                  </div>
              </form>
          </div>
      </div>
    </Teleport>
</template>


<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { useI18nStore } from '../stores/i18n'
import { useNotificationStore } from '../stores/notification'
import apiClient from '../services/apiClient'
import logo from '../assets/logo.svg'
import {
  Stethoscope, Building2, FileText, Users as UsersIcon, Package, BarChart3,
  LogOut, Search, ArrowRight, Sparkles, Bot, Shield, Wallet,
  User, KeyRound, X, ChevronDown, ChevronLeft, Bell, Check, ShieldAlert, Inbox,
  Building, ShieldCheck, CalendarCheck, Menu, UserRound, Activity, Calculator, CheckCircle2,
  LayoutDashboard, ClipboardList, UserCheck, Settings
} from 'lucide-vue-next'
import { useToast } from '../composables/useToast'

const toast = useToast()

// ── Role-Based Menu & Home Panels ───────────────────────────────────────────
import { useRoleMenu } from '../composables/useRoleMenu'
import AdminHomePanel    from '../components/home/AdminHomePanel.vue'
import DoctorHomePanel   from '../components/home/DoctorHomePanel.vue'
import QcHomePanel       from '../components/home/QcHomePanel.vue'
import FinanceHomePanel  from '../components/home/FinanceHomePanel.vue'
import ContractHomePanel from '../components/home/ContractHomePanel.vue'

// ── Module Views ─────────────────────────────────────────────────────────────
import { usePermission } from '../composables/usePermission'
import Companies        from './Companies.vue'
import Contracts        from './Contracts.vue'
import Staff            from './Staff.vue'
import Groups           from './Groups.vue'
import AnalyticsDashboard from './AnalyticsDashboard.vue'
import AccountManager   from './Users.vue'
import Permissions      from './Permissions.vue'
import SettlementReport from './SettlementReport.vue'
import Patients         from './Patients.vue'
import Payroll          from './Payroll.vue'
import Supplies         from './Supplies.vue'


const { can } = usePermission()

// ── Role-Based Menu ───────────────────────────────────────────────────────────
const {
  filteredMenuItems: roleFilteredMenuItems,
  homePanel,
  themeBg,
  accentClass,
  greeting: roleGreeting
} = useRoleMenu()

// Map panel name → component
const homePanelComponents = {
  AdminHomePanel,
  DoctorHomePanel,
  QcHomePanel,
  FinanceHomePanel,
  ContractHomePanel
}
const currentHomePanel = computed(() => homePanelComponents[homePanel.value] || AdminHomePanel)

const getRoleDisplayName = (role) => {
  return i18n.t('roles.' + role)
}

const getRoleBadgeClass = (role) => {
    switch(role) {
        case 'Admin': return 'bg-slate-900 text-white border-slate-700 shadow-slate-200'
        case 'MedicalGroupManager': return 'bg-primary/10 text-primary border-primary/20'
        case 'PersonnelManager': return 'bg-rose-50 text-rose-600 border-rose-100'
        case 'ContractManager': return 'bg-teal-50 text-teal-600 border-teal-100'
        case 'WarehouseManager': return 'bg-violet-50 text-violet-600 border-violet-100'
        case 'PayrollManager': return 'bg-emerald-50 text-emerald-600 border-emerald-100'
        case 'MedicalStaff': return 'bg-sky-50 text-sky-600 border-sky-100'
        case 'Customer': return 'bg-amber-50 text-amber-600 border-amber-100'
        default: return 'bg-slate-50 text-slate-500 border-slate-200'
    }
}


const router = useRouter()
const authStore = useAuthStore()
const i18n = useI18nStore()
const notificationStore = useNotificationStore()
const activeMenu = ref('home')
const isUserMenuOpen = ref(false)
const isSearchFocused = ref(false)
const showNotificationDropdown = ref(false)
const isMobileMenuOpen = ref(false)
const isSidebarCollapsed = ref(false)

// Password Change State
const showPasswordModal = ref(false)
const isChangingPass = ref(false)
const passForm = ref({ currentPassword: '', newPassword: '', confirmPassword: '' })

const handleChangePassword = async () => {
    if (passForm.value.newPassword !== passForm.value.confirmPassword) {
        alert("Mật khẩu mới không khớp!")
        return
    }

    isChangingPass.value = true
    try {
        const res = await apiClient.post('/api/Auth/change-password', {
            currentPassword: passForm.value.currentPassword,
            newPassword: passForm.value.newPassword
        })
        toast.success(res.data.message || "Đổi mật khẩu thành công!")
        showPasswordModal.value = false
        passForm.value = { currentPassword: '', newPassword: '', confirmPassword: '' }
    } catch (err) {
        toast.error(err.response?.data || "Lỗi khi đổi mật khẩu")
    } finally {
        isChangingPass.value = false
    }
}

// menuItems kept for breadcrumb/icon lookup & activeModules widget grid on Admin panel
const menuItems = computed(() => [
    { id: 'home',             name: 'Tổng quan',   icon: LayoutDashboard, color: 'bg-primary-light text-primary',    desc: 'Trung tâm điều chuyển.' },
    { id: 'companies',        name: 'Công ty',      icon: Building2,       color: 'bg-sky-50 text-sky-600',           desc: 'Quản lý thông tin công ty.',    permission: 'HopDong.View' },
    { id: 'contracts',        name: 'Hợp đồng',    icon: FileText,        color: 'bg-teal-50 text-teal-600',        desc: 'Pháp lý & Ký kết.',             permission: 'HopDong.View' },
    { id: 'groups',           name: 'Đoàn khám',   icon: Stethoscope,     color: 'bg-primary/10 text-primary',      desc: 'Vận hành tuyến đầu.',           permission: 'DoanKham.View' },
    { id: 'patients',         name: 'Bệnh nhân',   icon: UserRound,       color: 'bg-sky-50 text-sky-600',           desc: 'Hồ sơ sức khỏe.',               permission: 'DoanKham.View' },
    { id: 'supplies',         name: 'Vật tư',      icon: Package,         color: 'bg-violet-50 text-violet-600',    desc: 'Kho dược & Thiết bị.',          permission: 'Kho.View' },
    { id: 'settlement-report',name: 'Quyết toán',  icon: Calculator,      color: 'bg-emerald-50 text-emerald-600',  desc: 'Đối soát doanh thu.',           permission: 'BaoCao.View' },
    { id: 'staff',            name: 'Nhân sự',     icon: UsersIcon,       color: 'bg-rose-50 text-rose-600',        desc: 'Đội ngũ y tế.',                 permission: 'NhanSu.View' },
    { id: 'payroll',          name: 'Tính lương',  icon: Wallet,          color: 'bg-emerald-50 text-emerald-600',  desc: 'Thanh toán thu nhập.',          permission: 'Luong.View' },
    { id: 'analytics',        name: 'Thống kê',    icon: BarChart3,       color: 'bg-indigo-50 text-indigo-600',    desc: 'Báo cáo vận hành.',             permission: 'BaoCao.View' },
    { id: 'users',            name: 'Tài khoản',   icon: User,            color: 'bg-slate-50 text-slate-600',      desc: 'Quản trị truy cập.',            permission: 'HeThong.UserManage' },
    { id: 'permissions',      name: 'Phân quyền',  icon: ShieldCheck,     color: 'bg-slate-900 text-white',         desc: 'Bảo mật hệ thống.',             permission: 'HeThong.RoleManage' },
])

const activeModules = computed(() =>
    menuItems.value.filter(i => i.id !== 'home' && (!i.permission || can(i.permission)))
)

// Sidebar dùng filteredMenuItems từ useRoleMenu — đã được lọc theo role profile
const filteredMenuItems = roleFilteredMenuItems

const activeMenuName = computed(() => menuItems.value.find(i => i.id === activeMenu.value)?.name || '')
const itemsBreadcrumb = computed(() => activeMenu.value === 'home' ? (i18n.locale === 'vi' ? 'Hệ thống quản lý' : 'Management System') : (i18n.locale === 'vi' ? 'Module quản trị' : 'Admin Module'))
const activeIcon = computed(() => menuItems.value.find(i => i.id === activeMenu.value)?.icon || Bot)

const handleLogout = () => {
  authStore.logout()
  router.push('/login')
}

// Search Logic
const searchQuery = ref('')
const searchResults = ref([])
const isDataSyncing = ref(false)
const allData = ref({ companies: [], contracts: [], staff: [], groups: [] })

// Password Reset Requests (Admin only)
const resetRequests = ref([])
const showResetModal = ref(false)
const processingResetId = ref(null)
const newPasswordForReset = ref('HealthCare2026')

const fetchResetRequests = async () => {
    if (!authStore.isAdmin) return
    try {
        const res = await apiClient.get('/api/Auth/reset-requests')
        resetRequests.value = res.data
    } catch (err) {
        console.warn("Failed to fetch reset requests", err)
    }
}

const handleProcessReset = async (id) => {
    try {
        await apiClient.post('/api/Auth/process-reset', {
            id: id,
            newPassword: newPasswordForReset.value
        })
        toast.success(`Đã cấp lại mật khẩu mới cho tài khoản. Mật khẩu mặc định là: ${newPasswordForReset.value}`)
        fetchResetRequests()
    } catch (err) {
        toast.error("Lỗi khi xử lý yêu cầu")
    }
}

// Live Chart Data state
const liveStats = ref([
    { label: 'Check-in', value: 12, colorClass: 'bg-indigo-400' },
    { label: 'Tim Mạch', value: 35, colorClass: 'bg-rose-400' },
    { label: 'Siêu âm', value: 48, colorClass: 'bg-teal-400' },
    { label: 'X-Quang', value: 24, colorClass: 'bg-primary' },
    { label: 'Tai-Mũi-Họng', value: 15, colorClass: 'bg-violet-400' },
    { label: 'Tổng quát', value: 65, colorClass: 'bg-emerald-400' },
    { label: 'Kết luận', value: 10, colorClass: 'bg-amber-400' }
])

// Simulate Real-time Data Update
const updateLiveStats = () => {
    liveStats.value = liveStats.value.map(s => ({
        ...s,
        value: Math.max(5, Math.min(100, s.value + Math.floor(Math.random() * 11) - 5))
    }))
}

const fetchSearchData = async () => {
    if (isDataSyncing.value) return
    isDataSyncing.value = true
    try {
        const allEndpoints = [
            { key: 'companies', url: '/api/Companies', roles: ['Admin', 'ContractManager', 'MedicalStaff'] },
            { key: 'contracts', url: '/api/Contracts', roles: ['Admin', 'ContractManager', 'Customer', 'MedicalStaff'] },
            { key: 'staff', url: '/api/Staffs', roles: ['Admin', 'PersonnelManager', 'MedicalGroupManager', 'MedicalStaff'] },
            { key: 'groups', url: '/api/MedicalGroups', roles: ['Admin', 'MedicalGroupManager', 'MedicalStaff', 'Customer'] }
        ]

        const userRole = authStore.userRole?.toLowerCase()
        const endpoints = allEndpoints.filter(ep => ep.roles.some(r => r.toLowerCase() === userRole))

        await Promise.all(endpoints.map(async (ep) => {
            try {
                const res = await apiClient.get(ep.url)
                allData.value[ep.key] = res.data
            } catch (err) {
                console.warn(`Could not sync ${ep.key} for search`, err)
            }
        }))
    } catch (e) {
        console.error("Global search sync failed", e)
    } finally {
        isDataSyncing.value = false
    }
}

const handleSearch = () => {
    if (!searchQuery.value) {
        searchResults.value = []
        return
    }

    const q = searchQuery.value.toLowerCase().trim()
    const matches = []
    const matchesQuery = (text) => text?.toLowerCase().includes(q)

    if (Array.isArray(allData.value.companies)) {
        allData.value.companies.forEach(c => {
            if (matchesQuery(c.companyName)) matches.push({ id: c.companyId, name: c.companyName, type: 'company', target: 'companies' })
        })
    }
    if (Array.isArray(allData.value.contracts)) {
        allData.value.contracts.forEach(c => {
            if (matchesQuery(c.companyName)) matches.push({ id: c.healthContractId, name: `HĐ ${c.companyName}`, type: 'contract', target: 'contracts' })
        })
    }
    if (Array.isArray(allData.value.staff)) {
        allData.value.staff.forEach(s => {
            if (matchesQuery(s.fullName) || matchesQuery(s.jobTitle)) matches.push({ id: s.staffId, name: s.fullName, type: 'staff', target: 'staff' })
        })
    }
    if (Array.isArray(allData.value.groups)) {
        allData.value.groups.forEach(g => {
            if (matchesQuery(g.groupName)) matches.push({ id: g.medicalGroupId, name: g.groupName, type: 'group', target: 'groups' })
        })
    }

    searchResults.value = matches.slice(0, 8) 
}

const navigateSearchResult = (res) => {
    searchQuery.value = ''
    searchResults.value = []
    isSearchFocused.value = false
    activeMenu.value = res.target
}

const getSearchIcon = (type) => {
    switch(type) {
        case 'company': return Building2
        case 'contract': return FileText
        case 'staff': return UsersIcon
        case 'group': return Stethoscope
        default: return Bot
    }
}

const getResTypeName = (type) => {
    const map = { company: 'Cơ quan/Công ty', contract: 'Hợp đồng pháp lý', staff: 'Nhân sự y tế', group: 'Đoàn đang vận hành' }
    return map[type] || 'Dữ liệu hệ thống'
}

// Global Keyboard Shortcut (Ctrl+K)
const handleGlobalKeyDown = (e) => {
    if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
        e.preventDefault()
        const searchInput = document.querySelector('input[placeholder*="Ctrl+K"]')
        searchInput?.focus()
    }
    if (e.key === 'Escape') {
        isSearchFocused.value = false
        isUserMenuOpen.value = false
    }
}

// Today's Assignment Logic
const todayAssignment = ref(null)
const isCheckingIn = ref(false)
const showConfirmCheckIn = ref(false)

const fetchTodayAssignment = async () => {
    try {
        const res = await apiClient.get('/api/MedicalGroups/today-assignment')
        todayAssignment.value = res.data
    } catch (err) {
        console.warn("Failed to fetch today's assignment", err)
    }
}

const handleCheckInConfirm = () => {
    const action = !todayAssignment.value.checkInTime ? 'VÀO ĐOÀN' : 'RỜI ĐOÀN'
    if (confirm(`Bạn xác nhận thực hiện ${action} cho đoàn ${todayAssignment.value.groupName}?`)) {
        performQuickCheckIn()
    }
}

const performQuickCheckIn = async () => {
    isCheckingIn.value = true
    try {
        const res = await apiClient.post('/api/Attendance/self-checkin-direct', todayAssignment.value.groupId)
        toast.success(res.data.message)
        await fetchTodayAssignment() // Refresh data
        notificationStore.fetchNotifications()
    } catch (err) {
        toast.error(err.response?.data?.message || "Lỗi khi điểm danh")
    } finally {
        isCheckingIn.value = false
    }
}

onMounted(() => {
    fetchSearchData()
    fetchResetRequests()
    notificationStore.fetchNotifications()
    fetchTodayAssignment()
    
    // Poll for data every 30s
    const pollInterval = setInterval(() => {
        fetchResetRequests()
        notificationStore.fetchNotifications()
        fetchTodayAssignment()
    }, 30000)

    // Chart Data polling
    const chartInterval = setInterval(() => {
        if (activeMenu.value === 'home') {
            updateLiveStats()
        }
    }, 3000)

    window.addEventListener('keydown', handleGlobalKeyDown)
    document.addEventListener('click', (e) => {
        if (!e.target.closest('.group') && !e.target.closest('input')) {
            isSearchFocused.value = false
        }
        if (!e.target.closest('.notification-area')) {
            showNotificationDropdown.value = false
        }
    })
})
</script>

<style scoped>
.custom-scrollbar::-webkit-scrollbar {
  width: 6px;
  height: 6px;
}
.custom-scrollbar::-webkit-scrollbar-track {
  background: transparent;
}
.custom-scrollbar::-webkit-scrollbar-thumb {
  background: #e2e8f0;
  border-radius: 10px;
}
.custom-scrollbar::-webkit-scrollbar-thumb:hover {
  background: #cbd5e1;
}

@keyframes fade-in-up {
  from { opacity: 0; transform: translateY(20px); }
  to { opacity: 1; transform: translateY(0); }
}

.animate-fade-in-up {
  animation: fade-in-up 0.4s cubic-bezier(0.16, 1, 0.3, 1) forwards;
}

@keyframes scale-up {
  from { opacity: 0; transform: scale(0.95); }
  to { opacity: 1; transform: scale(1); }
}

.animate-scale-up {
  animation: scale-up 0.3s cubic-bezier(0.16, 1, 0.3, 1) forwards;
}
</style>
