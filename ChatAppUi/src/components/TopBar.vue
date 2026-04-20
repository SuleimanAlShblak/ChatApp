<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  title?: string
  showMenuButton?: boolean
  sticky?: boolean
  class?: string
}

const props = withDefaults(defineProps<Props>(), {
  showMenuButton: false,
  sticky: false,
})

const emit = defineEmits<{
  menuClick: []
}>()

const topbarClass = computed(() => {
  return [
    'topbar',
    {
      'topbar--sticky': props.sticky,
    },
    props.class,
  ]
    .filter(Boolean)
    .join(' ')
})
</script>

<template>
  <header class="topbar" :class="topbarClass">
    <div class="topbar-left">
      <button
        v-if="showMenuButton"
        class="topbar-menu-btn"
        @click="$emit('menuClick')"
        aria-label="Toggle menu"
      >
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M4 6h16M4 12h16M4 18h16"
          />
        </svg>
      </button>

      <div v-if="title || $slots.title" class="topbar-title">
        <slot name="title">
          <h1 class="topbar-title-text">{{ title }}</h1>
        </slot>
      </div>
    </div>

    <div class="topbar-center">
      <slot name="center" />
    </div>

    <div class="topbar-right">
      <slot name="actions" />
    </div>
  </header>
</template>

<style>
</style>
