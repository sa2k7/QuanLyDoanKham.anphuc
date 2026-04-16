<template>
  <header v-if="patient" class="patient-banner">
    <div class="p-basic">
      <h1>{{ patient.fullName }}</h1>
      <span class="p-meta">
        STT: {{ patient.queueNo }} • 
        Giới tính: {{ patient.gender }}
      </span>
    </div>
    <div class="p-status">
      <span class="status-badge" :class="patient.status?.toLowerCase()">
        {{ patient.status }}
      </span>
    </div>
  </header>
</template>

<script setup>
defineProps({
  patient: {
    type: Object,
    required: true
  }
})
</script>

<style scoped>
.patient-banner {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
}

.p-basic h1 {
  font-size: 2rem;
  font-weight: 900;
  color: #1e293b;
}

.p-meta {
  font-size: 0.875rem;
  color: #64748b;
  font-weight: 600;
}

.status-badge {
  padding: 0.5rem 1rem;
  border-radius: 9999px;
  font-size: 0.75rem;
  font-weight: 900;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.status-badge.waiting {
  background: #fef3c7;
  color: #b45309;
}

.status-badge.station_in_progress {
  background: #dcfce7;
  color: #15803d;
}
</style>
