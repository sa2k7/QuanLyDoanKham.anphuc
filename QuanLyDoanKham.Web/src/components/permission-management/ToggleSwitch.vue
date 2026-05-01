<template>
  <label class="toggle-switch-wrapper">
    <input
      type="checkbox"
      class="toggle-switch-input"
      :checked="modelValue"
      @change="$emit('update:modelValue', $event.target.checked)"
    />
    <div class="toggle-switch-track" :class="modelValue ? 'is-active' : ''">
      <div class="toggle-switch-thumb"></div>
    </div>
    <div class="toggle-switch-labels">
      <span class="toggle-switch-label" :class="modelValue ? 'is-active' : ''">
        {{ label }}
      </span>
      <code v-if="sublabel" class="toggle-switch-sublabel" :class="modelValue ? 'is-active' : ''">
        {{ sublabel }}
      </code>
    </div>
  </label>
</template>

<script setup>
defineProps({
  modelValue: { type: Boolean, default: false },
  label:      { type: String,  required: true },
  sublabel:   { type: String,  default: '' },
})

defineEmits(['update:modelValue'])
</script>

<style scoped>
.toggle-switch-wrapper {
  display: flex;
  align-items: center;
  gap: 12px;
  cursor: pointer;
  padding: 10px 14px;
  border-radius: 12px;
  border: 1px solid transparent;
  transition: all 0.2s ease;
}
.toggle-switch-wrapper:hover {
  background: rgba(255, 255, 255, 0.6);
}

.toggle-switch-input {
  position: absolute;
  width: 1px;
  height: 1px;
  overflow: hidden;
  clip: rect(0, 0, 0, 0);
}

/* Track */
.toggle-switch-track {
  position: relative;
  width: 40px;
  height: 22px;
  border-radius: 11px;
  background: #cbd5e1;
  transition: background 0.3s ease;
  flex-shrink: 0;
  box-shadow: inset 0 1px 3px rgba(0, 0, 0, 0.1);
}
.toggle-switch-track.is-active {
  background: #22c55e;
}

/* Thumb */
.toggle-switch-thumb {
  position: absolute;
  top: 3px;
  left: 3px;
  width: 16px;
  height: 16px;
  border-radius: 50%;
  background: #fff;
  transition: transform 0.3s cubic-bezier(0.34, 1.56, 0.64, 1);
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.15);
}
.toggle-switch-track.is-active .toggle-switch-thumb {
  transform: translateX(18px);
}

/* Labels */
.toggle-switch-labels {
  display: flex;
  flex-direction: column;
  min-width: 0;
}
.toggle-switch-label {
  font-size: 0.875rem;
  font-weight: 600;
  color: #94a3b8;
  transition: color 0.2s ease;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.toggle-switch-label.is-active {
  color: #1e293b;
}
.toggle-switch-sublabel {
  font-size: 10px;
  font-family: monospace;
  color: #cbd5e1;
  transition: color 0.2s ease;
  margin-top: 2px;
}
.toggle-switch-sublabel.is-active {
  color: rgba(99, 102, 241, 0.5);
}
</style>
