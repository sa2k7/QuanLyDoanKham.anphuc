> **BrainSync Context Pumper** 🧠
> Dynamically loaded for active file: `QuanLyDoanKham.Web\src\views\Dashboard.vue` (Domain: **Generic Logic**)

### 📐 Generic Logic Conventions & Fixes
- **[problem-fix] Patched security issue Mobile — parallelizes async operations for speed**: -     <!-- Sidebar Navigation -->
+     <!-- Mobile Backdrop -->
-     <aside class="w-72 bg-white/80 backdrop-blur-2xl border-r border-slate-100 flex flex-col h-screen sticky top-0 z-[60] shadow-[10px_0_30px_-15px_rgba(0,0,0,0.05)] flex-shrink-0">
+     <div v-if="isMobileMenuOpen" @click="isMobileMenuOpen = false" class="fixed inset-0 bg-slate-900/50 backdrop-blur-sm z-[55] md:hidden"></div>
-       <!-- Logo Section -->
+ 
-       <div class="p-6 pb-8 flex items-center gap-3 cursor-pointer group" @click="activeMenu = 'home'">
+     <!-- Sidebar Navigation -->
-         <div class="bg-white p-1 rounded-2xl transition-all group-hover:rotate-6 shadow-lg shadow-primary/20 flex-shrink-0 border border-slate-100">
+     <aside :class="['w-72 bg-white/95 md:bg-white/80 backdrop-blur-2xl border-r border-slate-100 flex flex-col h-screen fixed md:sticky top-0 z-[60] shadow-[10px_0_30px_-15px_rgba(0,0,0,0.05)] flex-shrink-0 transition-transform duration-300', isMobileMenuOpen ? 'translate-x-0' : '-translate-x-full md:translate-x-0']">
-           <img :src="logo" class="w-10 h-10 object-contain" alt="Logo" />
+       <!-- Logo Section -->
-         </div>
+       <div class="p-6 pb-8 flex items-center gap-3 cursor-pointer group" @click="activeMenu = 'home'">
-         <div>
+         <div class="bg-white p-1 rounded-2xl transition-all group-hover:rotate-6 shadow-lg shadow-primary/20 flex-shrink-0 border border-slate-100">
-           <h1 class="font-black text-lg text-slate-900 leading-tight tracking-tight">ĐA KHOA <span class="text-primary italic">AN PHÚC</span></h1>
+           <img :src="logo" class="w-10 h-10 object-contain" alt="Logo" />
-           <p class="text-[8px] font-black text-slate-400 uppercase tracking-[0.3em] mt-1">Hệ thống Điều hành</p>
+         </div>
-         </div>
+         <div>
-       </div>
+           <h1 class="font-black text-lg text-slate-900 leading-tight tracking-tight">ĐA KHOA <span class="text-primary italic">AN PHÚ
… [diff truncated]
- **[problem-fix] Patched security issue Permissions — parallelizes async operations for speed**: -               <Payroll v-if="activeMenu === 'payroll'" />
+               <Permissions v-if="activeMenu === 'permissions'" />
-               
+               <Payroll v-if="activeMenu === 'payroll'" />
-               <div v-if="!['companies', 'contracts', 'staff', 'groups', 'supplies', 'users', 'payroll'].includes(activeMenu)" class="flex flex-col items-center justify-center py-40 bg-white rounded-[4rem] border-4 border-dashed border-slate-50">
+               <Departments v-if="activeMenu === 'departments'" />
-                 <div class="w-24 h-24 bg-slate-50 rounded-full flex items-center justify-center mb-6">
+               <MySchedule v-if="activeMenu === 'my-schedule'" />
-                     <component :is="activeIcon" class="w-12 h-12 text-slate-100" />
+               
-                 </div>
+               <div v-if="!['companies', 'contracts', 'staff', 'groups', 'supplies', 'users', 'permissions', 'payroll', 'departments', 'my-schedule'].includes(activeMenu)" class="flex flex-col items-center justify-center py-40 bg-white rounded-[4rem] border-4 border-dashed border-slate-50">
-                 <p class="text-xl font-black text-slate-300 uppercase tracking-widest ">Module {{ activeMenuName }} đang sẵn sàng</p>
+                 <div class="w-24 h-24 bg-slate-50 rounded-full flex items-center justify-center mb-6">
-                 <button @click="activeMenu = 'home'" class="mt-8 px-8 py-4 bg-primary text-white rounded-[2rem] font-black uppercase tracking-widest shadow-lg shadow-primary/20 active:scale-95 transition-all">Quay lại trang chủ</button>
+                     <component :is="activeIcon" class="w-12 h-12 text-slate-100" />
-               </div>
+                 </div>
-           </div>
+                 <p class="text-xl font-black text-slate-300 uppercase tracking-widest ">Module {{ activeMenuName }} đang sẵn sàng</p>
-       </div>
+                 <button @click="activeMenu = 'home'" class="mt-8 px-8 py-4 bg-p
… [diff truncated]
- **[what-changed] Replaced auth Auth**: -         const res = await axios.get('/api/Auth/reset-requests')
+         const res = await apiClient.get('/api/Auth/reset-requests')
-             await axios.put(`/api/Auth/users/${form.value.username}`, payload)
+             await apiClient.put(`/api/Auth/users/${form.value.username}`, payload)
-             await axios.post('/api/Auth/register', payload)
+             await apiClient.post('/api/Auth/register', payload)
- **[problem-fix] Fixed null crash in ShieldCheck — prevents null/undefined runtime crashes**: -                             <span class="inline-flex items-center px-3 py-1 rounded-lg bg-indigo-50 text-indigo-600 text-[10px] font-black uppercase tracking-[0.3em]">
+                             <div class="flex flex-wrap gap-1">
-                                 {{ i18n.t('roles.' + u.roleName) }}
+                                 <span class="inline-flex items-center px-2 py-1 rounded-lg bg-indigo-50 text-indigo-600 text-[9px] font-black uppercase tracking-widest border border-indigo-100">
-                             </span>
+                                     {{ i18n.t('roles.' + u.roleName) }}
-                         </td>
+                                 </span>
-                         <td class="p-4 text-center">
+                                 <span v-for="secRole in u.roles" :key="secRole" class="inline-flex items-center px-2 py-1 rounded-lg bg-slate-50 text-slate-500 text-[9px] font-black uppercase tracking-widest border border-slate-200">
-                             <div class="flex items-center justify-center gap-3">
+                                     {{ i18n.t('roles.' + secRole) }}
-                                 <button @click="openEditModal(u)" class="btn-action-premium variant-indigo text-slate-400" title="Hiệu chỉnh">
+                                 </span>
-                                     <Edit3 class="w-5 h-5" />
+                             </div>
-                                 </button>
+                         </td>
-                                 <button v-if="u.username !== 'admin'" @click="handleDelete(u.username)" class="btn-action-premium variant-rose text-slate-400" title="Xóa tài khoản">
+                         <td class="p-4 text-center">
-                                     <Trash2 class="w-5 h-5" />
+                             <div class="flex items-center justify-center gap-3">
-                                 </button>
+                                 <button @click=
… [diff truncated]
- **[convention] Fixed null crash in String — prevents null/undefined runtime crashes — confirmed 3x**: -         <button @click="activeTab = 'pending'" 
+         <button @click="activeTab = 'draft'" 
-                          activeTab === 'pending' ? 'bg-gradient-to-r from-amber-400 to-amber-500 text-white shadow-amber-500/30' : 'bg-amber-50/30 text-amber-600 border border-amber-100/50 hover:bg-amber-50']">
+                          activeTab === 'draft' ? 'bg-gradient-to-r from-slate-500 to-slate-600 text-white shadow-slate-500/30' : 'bg-slate-50 border border-slate-200 text-slate-500 hover:bg-slate-100']">
-             <Clock class="w-4 h-4" />
+             <Edit3 class="w-4 h-4" />
-             <span>Chờ duyệt ({{ String(filteredList.pending.length).padStart(3, '0') }})</span>
+             <span>Nháp ({{ String(filteredList.draft.length).padStart(3, '0') }})</span>
-         <button @click="activeTab = 'approved'" 
+         <button @click="activeTab = 'pending'" 
-                          activeTab === 'approved' ? 'bg-gradient-to-r from-blue-500 to-indigo-500 text-white shadow-blue-500/30' : 'bg-blue-50/30 text-blue-600 border border-blue-100/50 hover:bg-blue-50']">
+                          activeTab === 'pending' ? 'bg-gradient-to-r from-amber-400 to-amber-500 text-white shadow-amber-500/30' : 'bg-amber-50/30 text-amber-600 border border-amber-100/50 hover:bg-amber-50']">
-             <CheckCircle class="w-4 h-4" />
+             <Clock class="w-4 h-4" />
-             <span>Đã phê duyệt ({{ String(filteredList.approved.length).padStart(3, '0') }})</span>
+             <span>Chờ duyệt ({{ String(filteredList.pending.length).padStart(3, '0') }})</span>
-         <button @click="activeTab = 'active'" 
+         <button @click="activeTab = 'approved'" 
-                          activeTab === 'active' ? 'bg-gradient-to-r from-violet-500 to-purple-500 text-white shadow-violet-500/30' : 'bg-violet-50/30 text-violet-600 border border-violet-100/50 hover:bg-violet-50']">
+                          activeTab === 'approved' ? 'bg-gradient-
… [diff truncated]
- **[how-it-works] Git hotspots: .windsurfrules(17x), QuanLyDoanKham.Web/src/views/Contracts.vue(17x), .brainsync/rules/brainsync_auto.md(14x), .cursor/active-context.md**: 
