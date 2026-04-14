<template>
  <button class="button" :class="buttonClass" :disabled="disabled" @click="handleClick">
    <component v-if="icon && isIcon" :is="icon" class="button-icon" />
    <span v-if="text && !isIcon">{{ text }}</span>
    <slot v-if="!text && !isIcon" />
  </button>
</template>

<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  text?: string
  icon?: any
  isIcon?: boolean
  variant?: 'primary' | 'secondary' | 'outline' | 'ghost' | 'danger'
  size?: 'sm' | 'md' | 'lg'
  disabled?: boolean
  class?: string
  active?: boolean
  data?: any
}

const props = withDefaults(defineProps<Props>(), {
  variant: 'primary',
  size: 'md',
  disabled: false,
  isIcon: false,
})

const emit = defineEmits<{
  click: [data?: any]
}>()

const buttonClass = computed(() => {
  return [
    'button',
    `button--${props.variant}`,
    `button--${props.size}`,
    {
      'button--disabled': props.disabled,
      'button--active': props.active,
      'button--icon-only': props.isIcon,
    },
    props.class,
  ]
    .filter(Boolean)
    .join(' ')
})

const handleClick = () => {
  if (!props.disabled) {
    emit('click', props.data)
  }
}
</script>

<style scoped>
.button {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  font-weight: 500;
  border-radius: 0.375rem;
  border: 1px solid transparent;
  cursor: pointer;
  transition: all 0.15s ease-in-out;
  text-decoration: none;
  outline: none;
}

.button:focus {
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

/* Variants */
.button--primary {
  background-color: #3b82f6;
  color: white;
  border-color: #3b82f6;
}

.button--primary:hover:not(.button--disabled) {
  background-color: #2563eb;
  border-color: #2563eb;
}

.button--secondary {
  background-color: #6b7280;
  color: white;
  border-color: #6b7280;
}

.button--secondary:hover:not(.button--disabled) {
  background-color: #4b5563;
  border-color: #4b5563;
}

.button--outline {
  background-color: white;
  color: #374151;
  border-color: #d1d5db;
}

.button--outline:hover:not(.button--disabled) {
  background-color: #f9fafb;
  border-color: #9ca3af;
}

.button--ghost {
  background-color: transparent;
  color: #374151;
  border-color: transparent;
}

.button--ghost:hover:not(.button--disabled) {
  background-color: #f3f4f6;
}

.button--danger {
  background-color: #ef4444;
  color: white;
  border-color: #ef4444;
}

.button--danger:hover:not(.button--disabled) {
  background-color: #dc2626;
  border-color: #dc2626;
}

/* Sizes */
.button--sm {
  padding: 0.375rem 0.75rem;
  font-size: 0.75rem;
  height: 1.75rem;
}

.button--md {
  padding: 0.5rem 1rem;
  font-size: 0.875rem;
  height: 2.25rem;
}

.button--lg {
  padding: 0.625rem 1.25rem;
  font-size: 1rem;
  height: 2.75rem;
}

/* States */
.button--disabled {
  opacity: 0.5;
  cursor: not-allowed;
  pointer-events: none;
}

.button--active {
  background-color: #1d4ed8;
  border-color: #1d4ed8;
}

.button--icon-only {
  padding: 0.5rem;
  width: 2.25rem;
  height: 2.25rem;
}

.button--icon-only.button--sm {
  width: 1.75rem;
  height: 1.75rem;
  padding: 0.375rem;
}

.button--icon-only.button--lg {
  width: 2.75rem;
  height: 2.75rem;
  padding: 0.625rem;
}

.button-icon {
  width: 1rem;
  height: 1rem;
}

.button--sm .button-icon {
  width: 0.75rem;
  height: 0.75rem;
}

.button--lg .button-icon {
  width: 1.25rem;
  height: 1.25rem;
}
</style>
