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

<style>
</style>
