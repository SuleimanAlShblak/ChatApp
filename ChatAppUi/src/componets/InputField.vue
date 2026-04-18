<template>
  <div class="input-field-container">
    <label v-if="label" :for="inputId" class="input-label">{{ label }}</label>
    <input
      :id="inputId"
      :type="type"
      :placeholder="placeholder"
      :value="modelValue"
      :disabled="disabled"
      :required="required"
      :class="inputClass"
      @input="emit('update:modelValue', ($event.target as HTMLInputElement).value); emit('input', $event)"
      @keydown.enter="$emit('enter', $event)"
      @blur="$emit('blur', $event)"
      @focus="$emit('focus', $event)"
    />
    <span v-if="error" class="input-error">{{ error }}</span>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  modelValue?: string | number
  type?: string
  placeholder?: string
  label?: string
  disabled?: boolean
  required?: boolean
  error?: string
  class?: string
  inputId?: string
}

const props = withDefaults(defineProps<Props>(), {
  type: 'text',
  modelValue: '',
  disabled: false,
  required: false,
})

const emit = defineEmits<{
  'update:modelValue': [value: string | number]
  input: [event: Event]
  blur: [event: FocusEvent]
  focus: [event: FocusEvent]
  enter: [event: KeyboardEvent]
}>()

const inputClass = computed(() => {
  return [
    'input-field',
    props.class,
    {
      'input-field--error': props.error,
      'input-field--disabled': props.disabled,
    },
  ]
    .filter(Boolean)
    .join(' ')
})

const inputId = computed(() => props.inputId || `input-${Math.random().toString(36).substr(2, 9)}`)
</script>

<style scoped>
.input-field-container {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.input-label {
  font-weight: 500;
  color: #374151;
}

.input-field {
  padding: 0.5rem 0.75rem;
  border: 1px solid #d1d5db;
  border-radius: 0.375rem;
  font-size: 0.875rem;
  transition:
    border-color 0.15s ease-in-out,
    box-shadow 0.15s ease-in-out;
}

.input-field:focus {
  outline: none;
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.input-field--error {
  border-color: #ef4444;
}

.input-field--error:focus {
  border-color: #ef4444;
  box-shadow: 0 0 0 3px rgba(239, 68, 68, 0.1);
}

.input-field--disabled {
  background-color: #f9fafb;
  cursor: not-allowed;
  opacity: 0.6;
}

.input-error {
  color: #ef4444;
  font-size: 0.75rem;
}
</style>
