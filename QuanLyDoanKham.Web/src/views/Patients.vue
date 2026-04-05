<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center space-y-4 sm:space-y-0">
      <div>
        <h1 class="text-2xl font-bold tracking-tight text-gray-900 dark:text-white">
          {{ $t('patient.title', 'Danh sách Bệnh nhân') }}
        </h1>
        <p class="text-sm text-gray-500 mt-1">
          {{ $t('patient.subtitle', 'Quản lý lý lịch cá nhân và lịch sử khám chữa bệnh') }}
        </p>
      </div>

      <div class="flex space-x-3">
        <button
          class="inline-flex items-center justify-center rounded-md border border-transparent bg-blue-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 sm:w-auto transition-colors duration-200"
          @click="openAddModal"
        >
          <UserPlus class="w-4 h-4 mr-2" />
          {{ $t('patient.add', 'Thêm Bệnh Nhân Mới') }}
        </button>
      </div>
    </div>

    <!-- Stats -->
    <div class="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-4">
      <div class="bg-white dark:bg-gray-800 overflow-hidden shadow rounded-lg border border-gray-100 dark:border-gray-700">
        <div class="p-5">
          <div class="flex items-center">
            <div class="flex-shrink-0">
              <Users class="h-6 w-6 text-gray-400" />
            </div>
            <div class="ml-5 w-0 flex-1 relative">
              <dt class="text-sm font-medium text-gray-500 dark:text-gray-400 truncate">Tổng bệnh nhân</dt>
              <dd class="mt-1 text-3xl font-semibold text-gray-900 dark:text-white">{{ patients.length }}</dd>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Table List -->
    <div class="bg-white dark:bg-gray-800 shadow rounded-lg border border-gray-200 dark:border-gray-700 overflow-hidden">
      <div v-if="loading" class="p-8 text-center text-gray-500">
        <Loader2 class="w-8 h-8 mx-auto animate-spin mb-4" />
        <p>Đang tải dữ liệu bệnh nhân...</p>
      </div>
      <div v-else-if="patients.length === 0" class="p-12 text-center text-gray-500">
        <p class="text-lg font-medium text-gray-900 dark:text-white mb-2">Chưa có dữ liệu</p>
        <p>Hiện hệ thống chưa lưu trữ bệnh nhân nào.</p>
        <button
          @click="openAddModal"
          class="mt-4 inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-blue-600 hover:bg-blue-700"
        >
          <UserPlus class="w-4 h-4 mr-2" />
          Tạo Bệnh nhân đầu tiên
        </button>
      </div>
      <div v-else class="overflow-x-auto">
        <table class="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
          <thead class="bg-gray-50 dark:bg-gray-700/50">
            <tr>
              <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Họ và tên</th>
              <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Năm sinh</th>
              <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Giới tính</th>
              <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Điện thoại</th>
              <th scope="col" class="relative px-6 py-3"><span class="sr-only">Hành động</span></th>
            </tr>
          </thead>
          <tbody class="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
            <tr v-for="patient in patients" :key="patient.patientId" class="hover:bg-gray-50 dark:hover:bg-gray-700/50 transition-colors">
              <td class="px-6 py-4 whitespace-nowrap">
                <div class="flex items-center">
                  <div class="flex-shrink-0 h-10 w-10">
                    <div class="h-10 w-10 rounded-full bg-blue-100 flex items-center justify-center">
                      <span class="text-blue-600 font-medium text-sm">{{ patient.fullName.charAt(0) }}</span>
                    </div>
                  </div>
                  <div class="ml-4">
                    <div class="text-sm font-medium text-gray-900 dark:text-white">{{ patient.fullName }}</div>
                    <div class="text-sm text-gray-500">{{ patient.identityCard || 'Chưa cung cấp CMND' }}</div>
                  </div>
                </div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                {{ formatDate(patient.dateOfBirth) }}
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                <span :class="patient.gender === 'Nam' ? 'bg-blue-100 text-blue-800' : 'bg-pink-100 text-pink-800'" class="px-2 py-1 text-xs font-medium rounded-full">
                  {{ patient.gender || 'Unknown' }}
                </span>
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ patient.phoneNumber }}</td>
              <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                <button class="text-blue-600 hover:text-blue-900 mr-3">Hồ sơ khám</button>
                <button class="text-red-600 hover:text-red-900" @click="confirmDelete(patient.patientId)">Xóa</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { Users, UserPlus, Loader2 } from 'lucide-vue-next'
import api from '../services/api'

const patients = ref([])
const loading = ref(false)

const loadPatients = async () => {
  loading.value = true
  try {
    const res = await api.get('/Patients')
    patients.value = res.data
  } catch (error) {
    console.error('Lỗi khi tải DS bệnh nhân', error)
  } finally {
    loading.value = false
  }
}

const formatDate = (dateString) => {
  if (!dateString) return 'N/A'
  const d = new Date(dateString)
  return `${d.getDate()}/${d.getMonth() + 1}/${d.getFullYear()}`
}

const openAddModal = () => {
    // Để tích hợp modal thêm mới sau
    alert("Tính năng Thêm Bệnh Nhân đang xây dựng Frontend Modal. Dữ liệu Test có thể được call bằng Postman.")
}

const confirmDelete = async (id) => {
    if(confirm('Bạn có chắc chắn muốn xóa lý lịch bệnh nhân này? Mọi hồ sơ khám của họ sẽ mất!')) {
        try {
            await api.delete(`/Patients/${id}`)
            await loadPatients()
        } catch(error) {
            alert("Lỗi khi xóa!")
        }
    }
}

onMounted(() => {
  loadPatients()
})
</script>
