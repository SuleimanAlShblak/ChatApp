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

<template>
  <button class="button" :class="buttonClass" :disabled="disabled" @click="handleClick">
    <component v-if="icon && isIcon" :is="icon" class="button-icon" />
    <span v-if="text && !isIcon">{{ text }}</span>
    <slot v-if="!text && !isIcon" />
  </button>
</template>

<style>
</style>
